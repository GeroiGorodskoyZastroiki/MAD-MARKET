using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class CartControls : MonoBehaviour
{
    [ReadOnly] public float SpeedInput = 0f,
    SteeringInput = 0f;

    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    public void OnMove(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        SpeedInput = value.y;
        SteeringInput = SpeedInput >= 0 ? value.x : -value.x;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!Cart.Movement.sprint & !Cart.Movement.cooldown)
                StartCoroutine(Cart.Movement.Sprint());
            else
                StartCoroutine(Cart.Movement.EndSprint());
        }
    }

    public void OnPickUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Ray ray = Cart.Camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("Items"));
            if (hit.collider)
            {
                var item = hit.collider.gameObject;
                if (item.TryGetComponent<Item>(out var _))
                    Cart.Items.PickUp(item);                    
            } 
        }
    }

    public void OnSelectItem(InputAction.CallbackContext context) =>
        Cart.Items.CurrentItem = Cart.Items.Items[context.action.controls.IndexOf(x => x == context.control)];

    public void OnThrow(InputAction.CallbackContext context) =>
        Cart.Items.Throw();

    public void OnUse(InputAction.CallbackContext context)
    {

    }
}
