using Netick.Unity;

public class ServerDeActive : NetworkBehaviour
{
    public override void NetworkStart()
    {
        if (!Sandbox.IsServer) return;
        gameObject.SetActive(false);
    }
}