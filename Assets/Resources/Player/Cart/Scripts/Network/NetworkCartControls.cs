using UnityEngine;
using Unity.Netcode;

public class NetworkCartControls : NetworkBehaviour
{
    CartControls controls;

    private void Awake() 
    {
        controls = GetComponent<CartControls>();

        if (NetworkManager.Singleton.IsHost) return;

        controls.OnMoveCallback += MoveRpc;
        controls.OnChargeCallback += ChargeRpc;
    }

    [Rpc(SendTo.Server)]
    public void MoveRpc(Vector2 value) => controls.Move(value);

    [Rpc(SendTo.Server)]
    public void ChargeRpc() => controls.Charge();
}
