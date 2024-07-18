using UnityEngine;

public class Cart : MonoBehaviour
{
    public CartMovement Movement { get; private set; }
    public CartItems Items { get; private set; }
    public CartCamera Camera { get; private set; }
    public CartControls Controls { get; private set; }
    public CartAttack Attack { get; private set; }
    public CartPush Push { get; private set; }
    public CartStun Stun { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }

    void Start()
	{
        Movement = GetComponent<CartMovement>();
        Items = GetComponent<CartItems>();
        Controls = GetComponent<CartControls>();
        Camera = GetComponentInChildren<CartCamera>();
        Attack = GetComponentInChildren<CartAttack>();
        Push = GetComponentInChildren<CartPush>();
        Stun = GetComponentInChildren<CartStun>();
        Rigidbody = GetComponent<Rigidbody2D>();
		Rigidbody.centerOfMass = new Vector2(0, 0);
        Movement.Cart = Items.Cart = Camera.Cart = Controls.Cart =
        Attack.Cart = Push.Cart = Stun.Cart = this;
	}
}
