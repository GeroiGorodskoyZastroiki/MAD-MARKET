using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class CartItems : MonoBehaviour
{
    #region Values
    [SerializeField] private float pickUpDistanse;
    [SerializeField] private float throwForce;
    [ReadOnly] private sbyte itemsCount = 6;
    [ReadOnly] public List<GameObject> Items;
    private List<Transform> itemsPoints;
    [ReadOnly] public GameObject CurrentItem;
    #endregion

    #region References
    [HideInInspector] public Cart Cart;
    #endregion

    private void Start() 
    {
        Items = new List<GameObject>(itemsCount);
        itemsPoints = new List<Transform>(itemsCount);
        itemsPoints.AddRange(GetComponentsInChildren<Transform>().Single(x => x.name =="ItemPoints").GetComponentsInChildren<Transform>().Skip(1));
    }

    private void Update() 
    {
        foreach (var item in Items)
        {
            item.transform.position = itemsPoints[Items.IndexOf(item)].position;
            item.transform.rotation = itemsPoints[Items.IndexOf(item)].rotation;
        }
    }

    public void PickUp()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("Items"));
        if (hit.collider)
        {
            var item = hit.collider.gameObject;
            if (item.TryGetComponent<Item>(out var _))
            {
                if (Items.Contains(item)) return;
                if (Items.Count == Items.Capacity) return;
                if (Vector3.Distance(item.transform.position, transform.position) > pickUpDistanse) return;

                Items.Add(item);
                CurrentItem = item;
            }              
        }
    }

    void Use()
    {

    }

    void Swap()
    {

    }

    public void Throw()
    {
        if (!CurrentItem) return;
        Items.Remove(CurrentItem);
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - CurrentItem.transform.position).normalized;
        CurrentItem.GetComponent<Rigidbody2D>().AddForce(direction * throwForce, ForceMode2D.Impulse);
        CurrentItem = null;
    }
}
