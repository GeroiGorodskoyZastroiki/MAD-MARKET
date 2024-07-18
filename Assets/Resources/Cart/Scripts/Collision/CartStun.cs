using System.Collections;
using UnityEngine;

public class CartStun : MonoBehaviour
{
    [SerializeField] float stunTime;

    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    private void OnCollisionEnter2D(Collision2D collision) //����� � ��������� ���������
    {
        if (collision.collider.transform.parent)
            if (collision.collider.transform.parent.TryGetComponent(out Cart attackingCart))
                if (collision.collider.gameObject.CompareTag("Attack") && attackingCart.Movement.CurrentSpeed >= attackingCart.Attack.CriticalSpeed)
                {
                    Cart.Push.Push(collision.GetContact(0));
                    StartCoroutine(Stun());
                }
    }

    public IEnumerator Stun()
    {
        Debug.Log("stun");
        Cart.Movement.enabled = Cart.Items.enabled = false;
        yield return new WaitForSeconds(stunTime);
        Cart.Movement.enabled = Cart.Items.enabled = true;
        yield break;
    }
}
