using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private List<Item> items = new List<Item>(); // ������ �������� ���������
    private List<Item> instantiatedItems = new List<Item>(); // ������ ����������������� ���������
    private Dictionary<Item, Item> prefabToInstanceMap = new Dictionary<Item, Item>(); // ����� �������� � ���������
    [SerializeField] private GameObject chestInventoryPanel; // UI-������ ��� ��������� �������
    [SerializeField] private Transform itemsContainer; // Transform, ���� ����� ����������� �������� � UI

    private bool isOpen = false;

    public List<Item> InstantiatedItems => instantiatedItems;

    // �������� �������
    public void OpenChest()
    {
        if (isOpen) return;

        isOpen = true;
        chestInventoryPanel.SetActive(true);

        // ��������� ��������� ������
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.OpenInventoryForChest();
        }

        // ���������� �������� ������� � UI
        DisplayItems();
    }

    // �������� �������
    public void CloseChest()
    {
        if (!isOpen) return;

        isOpen = false;
        chestInventoryPanel.SetActive(false);

        // ��������� ��������� ������
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.CloseInventoryForChest();
        }

        // ������� UI �������
        ClearItemsUI();
    }

    // ���������� �������� � UI
    private void DisplayItems()
    {
        ClearItemsUI();
        instantiatedItems.Clear();
        prefabToInstanceMap.Clear();

        foreach (Item itemPrefab in items)
        {
            // ������ ����� �������
            Item item = Instantiate(itemPrefab, itemsContainer);
            item.transform.localScale = Vector3.one;
            item.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 32);
            item.GetComponent<CanvasGroup>().alpha = 1f;
            item.GetComponent<CanvasGroup>().blocksRaycasts = true;
            item.SetChest(this);
            instantiatedItems.Add(item);
            prefabToInstanceMap[itemPrefab] = item; // ��������� ����� ������-���������
            Debug.Log($"Instantiated item {item.gameObject.name} in chest UI.");
        }
    }

    // ������� UI �������
    private void ClearItemsUI()
    {
        foreach (Transform child in itemsContainer)
        {
            Item item = child.GetComponent<Item>();
            if (item != null)
            {
                item.SetChest(null);
                Destroy(child.gameObject); // ���������� ����������������� ��������
            }
        }
    }

    // ���������� �������� � ������
    public void AddItem(Item item)
    {
        // �������, ������� �����������, ����� ���� ����������, ������� ��������� ��� ����� � ������ ��������
        Item itemToAdd = item;

        // ���� ������� ��� �������� ���������� (��������, ��������������� �� ���������), ������ ��� ����� ��� ������
        if (instantiatedItems.Contains(item))
        {
            Debug.LogWarning($"Item {item.gameObject.name} is an instance. Adding it directly to the chest. Consider using a prefab reference.");
        }

        items.Add(itemToAdd);
        if (isOpen)
        {
            DisplayItems();
        }
    }

    // �������� �������� �� �������
    public void RemoveItem(Item item)
    {
        Item itemPrefab = null;
        Item itemInstance = null;

        foreach (var pair in prefabToInstanceMap)
        {
            if (pair.Value == item)
            {
                itemPrefab = pair.Key;
                itemInstance = pair.Value;
                break;
            }
        }

        if (itemPrefab != null && itemInstance != null)
        {
            items.Remove(itemPrefab);
            instantiatedItems.Remove(itemInstance);
            prefabToInstanceMap.Remove(itemPrefab);

            // ������� ������� �� itemsContainer, �� �� ������ ��������
            // itemInstance.transform.SetParent(null); // ������� ��� ������

            Debug.Log($"Removed item {item.gameObject.name} from chest.");
        }
        else
        {
            Debug.LogWarning($"Could not find item {item.gameObject.name} in chest to remove.");
        }

        if (isOpen)
        {
            DisplayItems();
        }
    }
}