using UnityEngine;

public class CartPush : MonoBehaviour 
{
    [SerializeField] float pushForce;
    [SerializeField] float pushCounterforce;
    [SerializeField] Transform centerOfMass;

    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    private void OnCollisionEnter2D(Collision2D collision) //����� � ��������� ���������
    {
        if (collision.collider.transform.parent)
            if (collision.collider.transform.parent.TryGetComponent(out Cart attackingCart))
                if (collision.collider.gameObject.CompareTag("Attack") && attackingCart.Movement.CurrentSpeed >= attackingCart.Attack.CriticalSpeed)
                {
                    Push(collision.GetContact(0));
                    Rotate();
                }
    }

    public void Push(ContactPoint2D contactPoint) //�������� ��������
    {
        Debug.Log("push");
        //contactPoint.rigidbody.AddForceAtPosition(contactPoint.normal * (-pushCounterforce), contactPoint.point, ForceMode2D.Impulse);
        //Cart.Rigidbody.AddForceAtPosition(contactPoint.normal * pushForce, contactPoint.point, ForceMode2D.Impulse);
        contactPoint.rigidbody.GetComponent<Cart>().Movement.StopSprint();
        contactPoint.rigidbody.AddForce(contactPoint.normal * (-pushCounterforce), ForceMode2D.Impulse);
        Cart.Rigidbody.AddForce(contactPoint.normal * pushForce, ForceMode2D.Impulse);
    }

    public void Rotate()
    {
        Cart.Rigidbody.angularVelocity = pushForce;
        //Cart.Rigidbody.centerOfMass = centerOfMass.localPosition;
        //Cart.Rigidbody.AddTorque(pushForce * 100, ForceMode2D.Impulse);
        //Cart.Rigidbody.centerOfMass = new Vector2(0, 0);//����������
    }
}
