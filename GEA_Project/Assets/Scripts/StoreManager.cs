using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoreManager : MonoBehaviour
{
    public List<Item> storeItems = new List<Item>();
    public GameObject itemParent;
    public GameObject itemUIPrefab;

    void Start()
    {
        GenerateStoreItems();
        DisplayItems(storeItems);
    }

    private void GenerateStoreItems()
    {
        storeItems.Clear();
        for (int i = 0; i < 100; i++)
        {
            string itemName = $"Item_{i:D2}";
            storeItems.Add(new Item(itemName, Random.Range(1, 10)));
        }
    }

    public void DisplayItems(List<Item> itemsToDisplay)
    {
        foreach (Transform child in itemParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in itemsToDisplay)
        {
            GameObject itemUI = Instantiate(itemUIPrefab, itemParent.transform);
            itemUI.GetComponentInChildren<TMP_Text>().text = $"{item.itemName} ({item.quantity})";
        }
    }

    public void SearchLinear(string searchText)
    {
        List<Item> foundItems = new List<Item>();
        foreach (var item in storeItems)
        {
            if (item.itemName.Equals(searchText))
            {
                foundItems.Add(item);
                break;
            }
        }
        DisplayItems(foundItems);
    }

    public void SearchBinary(string searchText)
    {
        storeItems.Sort((a, b) => a.itemName.CompareTo(b.itemName));

        List<Item> foundItems = new List<Item>();
        int left = 0, right = storeItems.Count - 1;

        while (left <= right)
        {
            int mid = (left + right) / 2;
            int cmp = storeItems[mid].itemName.CompareTo(searchText);

            if (cmp == 0)
            {
                foundItems.Add(storeItems[mid]);
                break;
            }
            else if (cmp < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        DisplayItems(foundItems);
    }
}