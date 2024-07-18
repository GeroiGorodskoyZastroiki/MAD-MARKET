using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    public int MaxCartItems = 6;
    public int PlayerCount = 6;
    [SerializeField] List<GameObject> itemsPrefabs = new List<GameObject>();
    [ReadOnly] public List<Dictionary<ItemType, int>> ShoppingLists = new List<Dictionary<ItemType, int>>();

    private void Start() 
    {
        // if (itemsPrefabs.Count != System.Enum.GetValues(typeof(ItemType)).Length)
        // {
        //     Debug.LogError("ItemsManager: itemsPrefabs.Count != System.Enum.GetValues(typeof(ItemType)).Length");
        //     return;
        // }

        for (int i = 0; i < PlayerCount; i++)
            ShoppingLists.Add(GenerateShoppingList());

        Dictionary<ItemType, int> overallItemsCount = GetAllItemsFromShoppingLists(ShoppingLists);
        ReduceItemsCount(ref overallItemsCount, ShoppingLists);
        AddMissingItems(ref overallItemsCount);

        GenerateItems(overallItemsCount);
    }

    private void GenerateItems(Dictionary<ItemType, int> itemsCount)
    {
        var itemPoints = GameObject.FindGameObjectsWithTag("ItemPoint").ToList();
        foreach (var item in itemsCount)
        {
            for (int i = 0; i < item.Value; i++)
            {
                var itemPrefab = itemsPrefabs.FirstOrDefault(x => x.GetComponent<Item>().ItemType == item.Key);
                if (itemPrefab)
                {
                    var randomIndex = Random.Range(0, itemPoints.Count);
                    var itemPoint = itemPoints[randomIndex];
                    var instance = Instantiate(itemPrefab, itemPoint.transform.position, itemPoint.transform.rotation, transform);
                    itemPoints.RemoveAt(randomIndex);
                }
            }
        }
    }

    private Dictionary<ItemType, int> GenerateShoppingList()
    {
        var items = new List<ItemType>(System.Enum.GetValues(typeof(ItemType)) as ItemType[]);
        for (int i = 0; i < items.Count; i++)
        {
            int randomIndex = Random.Range(i, items.Count);
            (items[i], items[randomIndex]) = (items[randomIndex], items[i]);
        }

        var result = new Dictionary<ItemType, int>();
        var remaining = MaxCartItems;
        foreach (var itemType in items)
        {
            var count = Random.Range(1, remaining + 1);
            result[itemType] = count;
            remaining -= count;
            if (remaining <= 0)
                break;
        }
        return result;
    }

    private Dictionary<ItemType, int> GetAllItemsFromShoppingLists(List<Dictionary<ItemType, int>> shoppingLists)
    {
        var result = new Dictionary<ItemType, int>();
        foreach (var shoppingList in shoppingLists)
        {
            foreach (var item in shoppingList)
            {
                if (result.ContainsKey(item.Key))
                    result[item.Key] += item.Value;
                else
                    result.Add(item.Key, item.Value);
            }
        }
        return result;
    }

    private void ReduceItemsCount(ref Dictionary<ItemType, int> itemsCount, List<Dictionary<ItemType, int>> shoppingLists)
    {
        var keys = new List<ItemType>(itemsCount.Keys);
        foreach (var itemType in keys)
        {
            int minLimit = shoppingLists.Max(list => list.ContainsKey(itemType) ? list[itemType] : 0);
            int maxLimit = itemsCount[itemType];
            itemsCount[itemType] = Random.Range(minLimit, maxLimit + 1);
        }
    }

    private void AddMissingItems(ref Dictionary<ItemType, int> overallItemsCount)
    {
        foreach (var itemType in System.Enum.GetValues(typeof(ItemType)) as ItemType[])
            if (!overallItemsCount.ContainsKey(itemType))
                overallItemsCount[itemType] = Random.Range(1, PlayerCount + 1);
    }
}
