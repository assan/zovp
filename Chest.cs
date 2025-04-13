using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField]  List<Item> items = new List<Item>(); // ������ ��������� � �������
    [SerializeField]  GameObject chestInventoryPanel; // UI-������ ��� ��������� �������
    [SerializeField]  Transform itemsContainer; // Transform, ���� ����� ����������� �������� � UI
    [SerializeField]  GameObject itemUIPrefab; // ������ UI-�������� ��� ��������

    private bool isOpen = false;

    public List<Item> Items => items;

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
        foreach (Item item in items)
        {
            GameObject itemUI = Instantiate(itemUIPrefab, itemsContainer);
            ItemUI itemUIScript = itemUI.GetComponent<ItemUI>();
            if (itemUIScript != null)
            {
                itemUIScript.SetItem(item);
            }
        }
    }

    // ������� UI �������
    private void ClearItemsUI()
    {
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    // ���������� �������� � ������
    public void AddItem(Item item)
    {
        items.Add(item);
        if (isOpen)
        {
            DisplayItems();
        }
    }

    // �������� �������� �� �������
    public void RemoveItem(Item item)
    {
        items.Remove(item);
        if (isOpen)
        {
            DisplayItems();
        }
    }
}