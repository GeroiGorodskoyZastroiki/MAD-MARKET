using UnityEngine;
using UnityEngine.InputSystem;

public class CartControls : MonoBehaviour
{
    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    public void OnMove(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        Cart.Movement.SpeedInput = value.y;
        Cart.Movement.SteeringInput = Cart.Movement.SpeedInput >= 0 ? value.x : -value.x;
    }

    public void OnCharge(InputAction.CallbackContext context)
    {
        if (context.performed)
            Cart.Movement.Charge();
    }

    public void OnPickUp(InputAction.CallbackContext context)
    {
        if (context.performed)
            Cart.Items.PickUp();
    }

    public void OnSelectItem(InputAction.CallbackContext context) =>
        Cart.Items.CurrentItem = Cart.Items.Items[context.action.controls.IndexOf(x => x == context.control)];

    public void OnThrow(InputAction.CallbackContext context) =>
        Cart.Items.Throw();

    public void OnUse(InputAction.CallbackContext context)
    {

    }

    public void OnFocus(InputAction.CallbackContext context) =>
        transform.root.GetComponentInChildren<CameraController>().SetFocused(context.performed);
}
