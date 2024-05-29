using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Netick;
using Netick.Unity;
using static NetickUnityTransport;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection;
// ReSharper disable All

[CreateAssetMenu(fileName = "UnityTransportProvider", menuName = "Netick/Transport/UnityTransportProvider", order = 1)]
public class UnityTransportProvider : NetworkTransportProvider
{
    public override NetworkTransport MakeTransportInstance() => new NetickUnityTransport();
}

public static class NetickUnityTransportExt
{
    public static NetickUnityTransportEndPoint ToNetickEndPoint(this NetworkEndpoint networkEndpoint) =>
        new (networkEndpoint);
}

public unsafe class NetickUnityTransport : NetworkTransport
{
    public struct NetickUnityTransportEndPoint : IEndPoint
    {
        private NetworkEndpoint _endPoint;
        string IEndPoint.IPAddress => _endPoint.Address;
        int IEndPoint.Port => _endPoint.Port;

        public NetickUnityTransportEndPoint(NetworkEndpoint networkEndpoint) => _endPoint = networkEndpoint;

        public override string ToString() => _endPoint.Address;
    }
    
    private class NetickUnityTransportConnection : TransportConnection
    {
        private readonly NetickUnityTransport _transport;
        public NetworkConnection Connection;
        public override IEndPoint EndPoint => _transport._driver.GetRemoteEndpoint(Connection).ToNetickEndPoint();
        public override int Mtu => MaxPayloadSize;

        public int MaxPayloadSize;
        public NetickUnityTransport Transport => _transport;

        public NetickUnityTransportConnection(NetickUnityTransport transport) => _transport = transport;

        public override void Send(IntPtr ptr, int length)
        {
            if (!Connection.IsCreated)
                return;
            _transport._driver.BeginSend(NetworkPipeline.Null, Connection, out var networkWriter);
            networkWriter.WriteBytesUnsafe((byte*)ptr.ToPointer(), length);
            _transport._driver.EndSend(networkWriter);
        }
    }

    private NetworkDriver _driver;

    private readonly Dictionary<NetworkConnection, NetickUnityTransportConnection> _connectedPeers =
        new();

    private readonly Queue<NetickUnityTransportConnection> _freeConnections = new();
    private NetworkConnection _serverConnection;

    private NativeList<NetworkConnection> _connections;

    private BitBuffer _bitBuffer;
    private readonly byte* _bytesBuffer = (byte*)UnsafeUtility.Malloc(BytesBufferSize, 4, Allocator.Persistent);
    private const int BytesBufferSize = 2048;
    private readonly byte[] _connectionRequestBytes = new byte[200];
    private NativeArray<byte> _connectionRequestNative = new NativeArray<byte>(200, Allocator.Persistent);

    ~NetickUnityTransport()
    {
        UnsafeUtility.Free(_bytesBuffer, Allocator.Persistent);
        _connectionRequestNative.Dispose();
    }

    public override void Init()
    {
        _bitBuffer = new BitBuffer(createChunks: false);
        _driver = NetworkDriver.Create(new UDPNetworkInterface());
        _connections = new NativeList<NetworkConnection>(
            Engine.IsServer ? Engine.Config.MaxPlayers : 0, Allocator.Persistent);
    }

    public override void Run(RunMode mode, int port)
    {
        if (Engine.IsServer)
        {
            var endpoint = NetworkEndpoint.AnyIpv4.WithPort((ushort)port);

            if (_driver.Bind(endpoint) != 0)
            {
                Debug.LogError($"Failed to bind to port {port}");
                return;
            }

            _driver.Listen();
        }

        for (var i = 0; i < Engine.Config.MaxPlayers; i++)
            _freeConnections.Enqueue(new NetickUnityTransportConnection(this));
    }

    public override void Shutdown()
    {
        if (_driver.IsCreated)
            _driver.Dispose();
        _connections.Dispose();
    }

    public override void Connect(string address, int port, byte[] connectionData, int connectionDataLength)
    {
        var endpoint = NetworkEndpoint.Parse(address, (ushort)port);
        if (connectionData != null)
        {
            _connectionRequestNative.CopyFrom(connectionData);
            _serverConnection = _driver.Connect(endpoint, _connectionRequestNative);
        }
        else
            _serverConnection = _driver.Connect(endpoint);
    }

    public override void Disconnect(TransportConnection connection)
    {
        var conn = (NetickUnityTransportConnection)connection;
        if (conn.Connection.IsCreated)
            _driver.Disconnect(conn.Connection);
    }

    public override void PollEvents()
    {
        _driver.ScheduleUpdate().Complete();

        if (Engine.IsClient && !_serverConnection.IsCreated)
            return;

        // reading events
        if (Engine.IsServer)
        {
            // clean up connections.
            for (var i = 0; i < _connections.Length; i++)
            {
                if (_connections[i].IsCreated) continue;
                _connections.RemoveAtSwapBack(i);
                i--;
            }

            // accept new connections in the server.
            NetworkConnection c;
            while ((c = _driver.Accept(out var payload)) != default)
            {
                if (_connectedPeers.Count >= Engine.Config.MaxPlayers)
                {
                    _driver.Disconnect(c);
                    continue;
                }

                if (payload.IsCreated)
                    payload.CopyTo(_connectionRequestBytes);
                var accepted = NetworkPeer.OnConnectRequest(_connectionRequestBytes, payload.Length,
                    _driver.GetRemoteEndpoint(c).ToNetickEndPoint());

                if (!accepted)
                {
                    _driver.Disconnect(c);
                    continue;
                }

                var connection = _freeConnections.Dequeue();
                connection.Connection = c;
                _connectedPeers.Add(c, connection);
                _connections.Add(c);

                connection.MaxPayloadSize = NetworkParameterConstants.MTU - _driver.MaxHeaderSize(NetworkPipeline.Null);
                NetworkPeer.OnConnected(connection);
            }

            for (var i = 0; i < _connections.Length; i++)
                HandleConnectionEvents(_connections[i], i);
        }
        else
            HandleConnectionEvents(_serverConnection, 0);
    }


    private void HandleConnectionEvents(NetworkConnection conn, int index)
    {
        NetworkEvent.Type cmd;

        while ((cmd = _driver.PopEventForConnection(conn, out var stream)) != NetworkEvent.Type.Empty)
        {
            switch (cmd)
            {
                // game data
                case NetworkEvent.Type.Data:
                {
                    if (_connectedPeers.TryGetValue(conn, out var netickConn))
                    {
                        stream.ReadBytesUnsafe(_bytesBuffer, stream.Length);
                        _bitBuffer.SetFrom(_bytesBuffer, stream.Length, BytesBufferSize);
                        NetworkPeer.Receive(netickConn, _bitBuffer);
                    }

                    break;
                }
                // connected to server
                case NetworkEvent.Type.Connect when Engine.IsClient:
                {
                    var connection = _freeConnections.Dequeue();
                    connection.Connection = conn;

                    _connectedPeers.Add(conn, connection);
                    _connections.Add(conn);

                    connection.MaxPayloadSize = NetworkParameterConstants.MTU - _driver.MaxHeaderSize(NetworkPipeline.Null);
                    NetworkPeer.OnConnected(connection);
                    break;
                }
                // disconnect
                case NetworkEvent.Type.Disconnect:
                {
                    if (_connectedPeers.TryGetValue(conn, out var netickConn))
                    {
                        const TransportDisconnectReason reason = TransportDisconnectReason.Shutdown;
                        NetworkPeer.OnDisconnected(netickConn, reason);
                        _freeConnections.Enqueue(netickConn);
                        _connectedPeers.Remove(conn);
                    }

                    if (Engine.IsClient)
                        _serverConnection = default;
                    if (Engine.IsServer)
                        _connections[index] = default;
                    break;
                }
                case NetworkEvent.Type.Empty:
                default:
                    break;
            }
        }
    }
}