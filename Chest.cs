using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private List<Item> items = new List<Item>(); // Список префабов предметов
    private List<Item> instantiatedItems = new List<Item>(); // Список инстанциированных предметов
    private Dictionary<Item, Item> prefabToInstanceMap = new Dictionary<Item, Item>(); // Связь префабов и инстанций
    [SerializeField] private GameObject chestInventoryPanel; // UI-панель для инвентаря сундука
    [SerializeField] private Transform itemsContainer; // Transform, куда будут добавляться предметы в UI

    private bool isOpen = false;

    public List<Item> InstantiatedItems => instantiatedItems;

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
        instantiatedItems.Clear();
        prefabToInstanceMap.Clear();

        foreach (Item itemPrefab in items)
        {
            // Создаём копию префаба
            Item item = Instantiate(itemPrefab, itemsContainer);
            item.transform.localScale = Vector3.one;
            item.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 32);
            item.GetComponent<CanvasGroup>().alpha = 1f;
            item.GetComponent<CanvasGroup>().blocksRaycasts = true;
            item.SetChest(this);
            instantiatedItems.Add(item);
            prefabToInstanceMap[itemPrefab] = item; // Сохраняем связь префаб-инстанция
            Debug.Log($"Instantiated item {item.gameObject.name} in chest UI.");
        }
    }

    // Очищаем UI сундука
    private void ClearItemsUI()
    {
        foreach (Transform child in itemsContainer)
        {
            Item item = child.GetComponent<Item>();
            if (item != null)
            {
                item.SetChest(null);
                Destroy(child.gameObject); // Уничтожаем инстанциированные предметы
            }
        }
    }

    // Добавление предмета в сундук
    public void AddItem(Item item)
    {
        // Предмет, который добавляется, может быть инстанцией, поэтому добавляем его копию в список префабов
        Item itemToAdd = item;

        // Если предмет уже является инстанцией (например, перетаскивается из инвентаря), создаём его копию как префаб
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

    // Удаление предмета из сундука
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

            // Удаляем предмет из itemsContainer, но не меняем родителя
            // itemInstance.transform.SetParent(null); // Удаляем эту строку

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