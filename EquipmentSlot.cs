using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemType slotType; // ��� ����� (Head, Torso � �.�.)
    public Item equippedItem; // ������� ������������� �������
    public Image slotImage; // ��� ��������� ����� ��� ���������
    public Inventory inventory; // ������ �� ���������

    void Start()
    {
        slotImage = GetComponent<Image>();
        inventory = FindObjectOfType<Inventory>();
        if (equippedItem != null && inventory != null)
        {
            // ���� ��������� ������ ��� �������������� ��������
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
            slotImage.color = Color.green; // ������������ ����
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        slotImage.color = Color.black; // ���������� ����
    }

    // ����� ��� �������� �������� � ���������
    public void ReturnItemToInventory(Item item)
    {
        // ������� ��������� ����� � ���������
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
        // ���� ����� ���, ����� ��������� ������� ��� ��������� ������
        Debug.LogWarning("��� ����� � ��������� ��� �������� ��������!");
    }

    // ����� ��� ������ ��������
    public void UnequipItem()
    {
        if (equippedItem == null) return;
        ReturnItemToInventory(equippedItem);
        equippedItem = null;
        FindObjectOfType<Player>().UpdateStats();
        Player.instance.UpdateStats();
    }
}