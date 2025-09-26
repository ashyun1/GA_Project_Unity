using UnityEngine;
using TMPro;

public class SearchUI : MonoBehaviour
{
    public StoreManager storeManager;
    public TMP_InputField searchInputField;

    public void OnLinearSearchButton()
    {
        string searchText = searchInputField.text;
        storeManager.SearchLinear(searchText);
    }

    public void OnBinarySearchButton()
    {
        string searchText = searchInputField.text;
        storeManager.SearchBinary(searchText);
    }

    public void OnShowAllItemsButton()
    {
        storeManager.DisplayItems(storeManager.storeItems);
    }
}