using UnityEngine;

public class CartAttack : MonoBehaviour
{
    public float CriticalSpeed;

    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.transform.parent && collision.collider.transform.parent.parent)
            if (collision.collider.transform.parent.parent.TryGetComponent(out Cart attackingCart))
                if (collision.collider.gameObject.CompareTag("Attack") && 
                    attackingCart.Movement.CurrentSpeed >= attackingCart.Attack.CriticalSpeed)
                    {
                        Debug.Log("GotAttacked");
                        Cart.Push.Push(collision.GetContact(0));
                        Cart.Stun.Stun();
                    }

        //Cart.Movement.StopSprint();
        if (Cart.Movement.CurrentSpeed >= CriticalSpeed && collision.collider.gameObject.layer == LayerMask.NameToLayer("StaticGeometry"))
        {
            Cart.Push.Push(collision.GetContact(0));
            StartCoroutine(Cart.Stun.Stun());
        }
    }
}
