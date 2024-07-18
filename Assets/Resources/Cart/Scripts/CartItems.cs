using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class CartItems : MonoBehaviour
{
    [SerializeField] private float pickUpDistanse;
    [SerializeField] private float throwForce;
    [ReadOnly] private sbyte itemsCount = 6;
    [ReadOnly] public List<GameObject> Items;
    private List<Transform> itemsPoints;
    [ReadOnly] public GameObject CurrentItem;

    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    private void Start() {
        Items = new List<GameObject>(itemsCount);
        itemsPoints = new List<Transform>(itemsCount);
        itemsPoints.AddRange(GetComponentsInChildren<Transform>().Single(x => x.name =="ItemPoints").GetComponentsInChildren<Transform>().Skip(1));
    }

    private void Update() {
        foreach (var item in Items)
        {
            item.transform.position = itemsPoints[Items.IndexOf(item)].position;
            item.transform.rotation = itemsPoints[Items.IndexOf(item)].rotation;
        }
            
    }

    public void PickUp(GameObject item)
    {
        if (Items.Contains(item)) return;
        if (Items.Count == Items.Capacity) return;
        if (Vector3.Distance(item.transform.position, transform.position) > pickUpDistanse) return;

        Items.Add(item);
        CurrentItem = item;
    }

    void Use()
    {

    }

    void OnSwap()
    {

    }

    public void Throw()
    {
        if (!CurrentItem) return;
        Items.Remove(CurrentItem);
        Vector2 direction = (Cart.Camera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition) - CurrentItem.transform.position).normalized;
        CurrentItem.GetComponent<Rigidbody2D>().AddForce(direction * throwForce, ForceMode2D.Impulse);
        CurrentItem = null;
    }
}
