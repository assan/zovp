using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemType slotType; // Тип слота (Head, Torso и т.д.)
    public Item equippedItem; // Текущий экипированный предмет
    public Image slotImage; // Для изменения цвета при наведении
    public Inventory inventory; // Ссылка на инвентарь

    void Start()
    {
        slotImage = GetComponent<Image>();
        inventory = FindObjectOfType<Inventory>();
        if (equippedItem != null && inventory != null)
        {
            // Ищем свободную ячейку для экипированного предмета
            for (int y = 0; y < inventory.ScalY; y++)
            {
                for (int x = 0; x < inventory.ScalX; x++)
                {
                    var cell = inventory.cells[x, y];
                    if (inventory.CheckCellFree(cell, equippedItem.GetSize()))
                    {
                        equippedItem.lastInventoryCell = cell;
                        Debug.Log($"Assigned lastInventoryCell ({x}, {y}) to item {equippedItem.gameObject.name} in slot {gameObject.name}");
                        break;
                    }
                }
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = eventData.pointerDrag.GetComponent<Item>();
        if (draggedItem == null) return;

        if (draggedItem.itemType == slotType)
        {
            if (equippedItem != null)
            {
                ReturnItemToInventory(equippedItem);
            }

            equippedItem = draggedItem;
            draggedItem.transform.SetParent(transform);
            draggedItem.transform.localPosition = Vector3.zero;
            draggedItem.prefcell = null;
            inventory.UpdateCellsColor();
            FindObjectOfType<Player>().UpdateStats();


        }
        else
        {
            draggedItem.SetPosition(draggedItem, draggedItem.prefcell);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventory.DragedItem && inventory.DragedItem.itemType == slotType)
        {
            slotImage.color = Color.green; // Подсвечиваем слот
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        slotImage.color = Color.black; // Возвращаем цвет
    }

    // Метод для возврата предмета в инвентарь
    public void ReturnItemToInventory(Item item)
    {
        // Находим свободное место в инвентаре
        for (int y = 0; y < inventory.ScalY; y++)
        {
            for (int x = 0; x < inventory.ScalX; x++)
            {
                var cell = inventory.cells[x, y];
                if (inventory.CheckCellFree(cell, item.GetSize()))
                {
                    item.SetPosition(item, cell);
                    item.prefcell = cell;
                    inventory.CellOkupation(cell, item.GetSize(), false);
                    inventory.UpdateCellsColor();
                    return;
                }
            }
        }
        // Если места нет, можно выбросить предмет или уведомить игрока
        Debug.LogWarning("Нет места в инвентаре для возврата предмета!");
    }

    // Метод для снятия предмета
    public void UnequipItem()
    {
        if (equippedItem == null) return;
        ReturnItemToInventory(equippedItem);
        equippedItem = null;
        FindObjectOfType<Player>().UpdateStats();
        Player.instance.UpdateStats();
    }
}