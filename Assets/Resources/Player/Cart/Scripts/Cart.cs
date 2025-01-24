using Unity.Netcode.Components;
using UnityEngine;

public class Cart : MonoBehaviour //
{
    public CartMovement Movement { get; private set; }
    public CartItems Items { get; private set; }
    public CartControls Controls { get; private set; }
    public CartAttack Attack { get; private set; }
    public CartPush Push { get; private set; }
    public CartStun Stun { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }

    void Awake()
	{
        Movement = GetComponentInChildren<CartMovement>();
        Items = GetComponentInChildren<CartItems>();
        Controls = GetComponentInChildren<CartControls>();
        Attack = GetComponentInChildren<CartAttack>();
        Push = GetComponentInChildren<CartPush>();
        Stun = GetComponentInChildren<CartStun>();

        Movement.Cart = Items.Cart = Controls.Cart =
        Attack.Cart = Push.Cart = Stun.Cart = this;

        Rigidbody = GetComponent<Rigidbody2D>();
		Rigidbody.centerOfMass = new Vector2(0, 0);
	}
}
