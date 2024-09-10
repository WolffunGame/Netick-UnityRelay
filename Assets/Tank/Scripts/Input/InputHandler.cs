using System.Diagnostics.CodeAnalysis;
using Netick;
using Netick.Unity;
using UnityEngine;

public class InputHandler : NetworkEventsListener
{
    [SerializeField] private LayerMask _mouseRayMask;
    private Transform _player;
    private uint _buttonReset;
    private uint _buttonSample;

    private Vector2 _moveDelta;
    private Vector2 _aimDelta;

    private Camera _cam;

    private void Start() => _cam ??= Camera.main;

    public void SetPlayer(Transform player)
    {
        _player = player;
    } 

    public override void OnInput(NetworkSandbox sandbox)
    {
        if (!_player)
            return;
        var input = sandbox.GetInput<InputData>();
        input.SetAimDirection(_aimDelta.normalized);
        input.SetMoveDirection(_moveDelta.normalized);
        input.Tick = sandbox.Tick.TickValue;
        if (_buttonSample != 0)
        {
            input.Buttons = _buttonSample;
            input.Buttons = _buttonSample;
            _buttonReset |= _buttonSample; 
            _buttonSample = 0;
        }
        sandbox.SetInput(input);
    }

    private void Update()
    {
        if(!_player || !Sandbox || !Sandbox.IsClient)
            return;
        _buttonSample &= ~_buttonReset;
        
        
        if (Input.GetMouseButton(0))
            _buttonSample |= InputData.BUTTON_FIRE_PRIMARY;

        if (Input.GetMouseButtonDown(1))
            _buttonSample |= InputData.BUTTON_FIRE_SECONDARY;

        if (Input.GetKey(KeyCode.R))
            _buttonSample |= InputData.BUTTON_TOGGLE_READY;

        _moveDelta = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
            _moveDelta += Vector2.up;

        if (Input.GetKey(KeyCode.S))
            _moveDelta += Vector2.down;

        if (Input.GetKey(KeyCode.A))
            _moveDelta += Vector2.left;

        if (Input.GetKey(KeyCode.D))
            _moveDelta += Vector2.right;
        var mousePos = Input.mousePosition;
        
        var view = _cam.ScreenToViewportPoint(mousePos);
        var isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
        if (isOutside) return;
        var ray = _cam.ScreenPointToRay(mousePos);
        var mouseCollisionPoint = Vector3.zero;
        // RayCast towards the mouse collider box in the world
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, _mouseRayMask))
            if (hit.collider != null)
                mouseCollisionPoint = hit.point;
        var aimDirection = mouseCollisionPoint - _player.position;
        _aimDelta = new Vector2(aimDirection.x, aimDirection.z);
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public struct InputData : INetworkInput
{
    public const uint BUTTON_FIRE_PRIMARY = 1 << 0;
    public const uint BUTTON_FIRE_SECONDARY = 1 << 1;
    public const uint BUTTON_TOGGLE_READY = 1 << 2;
    public byte EncodedMoveDir;
    public byte EncodedAimDir;
    public uint Buttons;
    public int Tick;

    public bool IsUp(uint button) => IsDown(button) == false;
    public bool IsDown(uint button) => (Buttons & button) == button;

    public bool WasPressed(uint button, InputData oldInput)
        => (oldInput.Buttons & button) == 0 && (Buttons & button) == button;

    public void SetMoveDirection(Vector2 direction) => EncodedMoveDir =  EncodeDir.EncodeDirection(direction);

    public Vector2 GetMoveDirection() => EncodeDir.DecodeDirection(EncodedMoveDir);

    public void SetAimDirection(Vector2 direction) => EncodedAimDir = EncodeDir.EncodeDirection(direction);

    public  Vector2 GetAimDirection() => EncodeDir.DecodeDirection(EncodedAimDir);
}

public static class PlayerInputExtensions
{
    
}