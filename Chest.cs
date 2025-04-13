using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField]  List<Item> items = new List<Item>(); // Список предметов в сундуке
    [SerializeField]  GameObject chestInventoryPanel; // UI-панель для инвентаря сундука
    [SerializeField]  Transform itemsContainer; // Transform, куда будут добавляться предметы в UI
    [SerializeField]  GameObject itemUIPrefab; // Префаб UI-элемента для предмета

    private bool isOpen = false;

    public List<Item> Items => items;

    // Открытие сундука
    public void OpenChest()
    {
        if (isOpen) return;

        isOpen = true;
        chestInventoryPanel.SetActive(true);

        // Открываем инвентарь игрока
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.OpenInventoryForChest();
        }

        // Отображаем предметы сундука в UI
        DisplayItems();
    }

    // Закрытие сундука
    public void CloseChest()
    {
        if (!isOpen) return;

        isOpen = false;
        chestInventoryPanel.SetActive(false);

        // Закрываем инвентарь игрока
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.CloseInventoryForChest();
        }

        // Очищаем UI сундука
        ClearItemsUI();
    }

    // Отображаем предметы в UI
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

    // Очищаем UI сундука
    private void ClearItemsUI()
    {
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    // Добавление предмета в сундук
    public void AddItem(Item item)
    {
        items.Add(item);
        if (isOpen)
        {
            DisplayItems();
        }
    }

    // Удаление предмета из сундука
    public void RemoveItem(Item item)
    {
        items.Remove(item);
        if (isOpen)
        {
            DisplayItems();
        }
    }
}