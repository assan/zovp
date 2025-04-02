using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerClickHandler
{
    public ItemSize Size;
    public ItemType itemType;
    Inventory inventory;
    RectTransform rectTransform;
   public CanvasGroup canvasGroup;
    Vector2 positionItem;
    public Cell prefcell;
    public Armory Armory;
    public Canvas canvas;
    public Cell lastInventoryCell;
    public float defenseBonus; // Бонус к защите
    public float attackBonus;  // Бонус к атаке
    


    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        Armory = FindObjectOfType<Armory>();
        inventory = Inventory.instance;
        if (inventory == null)
        {
            Debug.LogError($"Inventory not found for item: {gameObject.name}");
        }
    }

    void Update()
    {
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"OnBeginDrag called for item: {gameObject.name}, prefcell: {(prefcell != null ? prefcell.gameObject.name : "null")}, lastInventoryCell: {(lastInventoryCell != null ? lastInventoryCell.gameObject.name : "null")}");
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        if (inventory == null || !inventory.gameObject.activeInHierarchy)
        {
            Debug.LogError($"Inventory is null or not active in OnBeginDrag for item: {gameObject.name}");
            return;
        }

        inventory.DragedItem = this;

        EquipmentSlot currentSlot = transform.parent.GetComponent<EquipmentSlot>();
        if (currentSlot != null)
        {
            Debug.Log($"Item was in EquipmentSlot: {currentSlot.gameObject.name}");
            currentSlot.equippedItem = null;
            transform.SetParent(canvas.transform);

            if (lastInventoryCell != null)
            {
                Debug.Log($"Freeing cells starting from lastInventoryCell: {lastInventoryCell.gameObject.name}");
                inventory.CellOkupation(lastInventoryCell, GetSize(), true);
            }
            else
            {
                Debug.LogWarning($"lastInventoryCell is null for item {gameObject.name}. Cannot free cells.");
            }

            if (Player.instance != null && Player.instance.gameObject.activeInHierarchy)
            {
                Debug.Log($"Updating player stats after removing item {gameObject.name} from slot {currentSlot.gameObject.name}");
                Player.instance.UpdateStats();
            }
            else
            {
                Debug.LogWarning("Player not found or not active in OnBeginDrag!");
            }
        }
        else if (prefcell != null)
        {
            Debug.Log($"Freeing cells starting from prefcell: {prefcell.gameObject.name}");
            inventory.CellOkupation(prefcell, GetSize(), true);
        }

        inventory.UpdateCellsColor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (inventory == null || !inventory.gameObject.activeInHierarchy)
        {
            Debug.LogError($"Inventory is null or not active in OnEndDrag for item: {gameObject.name}");
            return;
        }

        inventory.DragedItem = null;

        GameObject droppedObject = eventData.pointerCurrentRaycast.gameObject;
        Cell targetCell = null;
        EquipmentSlot targetSlot = null;

        if (droppedObject != null)
        {
            targetCell = droppedObject.GetComponent<Cell>();
            targetSlot = droppedObject.GetComponent<EquipmentSlot>();
        }

        if (targetSlot != null && targetSlot.slotType == itemType)
        {
            if (targetSlot.equippedItem != null)
            {
                targetSlot.ReturnItemToInventory(targetSlot.equippedItem);
            }
            targetSlot.equippedItem = this;
            transform.SetParent(targetSlot.transform);
            transform.localPosition = Vector3.zero;
            // Сохраняем последнюю ячейку инвентаря перед экипировкой
            if (prefcell != null)
            {
                lastInventoryCell = prefcell;
            }
            prefcell = null;
            if (Player.instance != null && Player.instance.gameObject.activeInHierarchy)
            {
                Player.instance.UpdateStats();
            }
            inventory.UpdateCellsColor();
            return;
        }

        if (targetCell != null)
        {
            if (inventory.CheckCellFree(targetCell, GetSize()))
            {
                SetPosition(this, targetCell);
                prefcell = targetCell;
                lastInventoryCell = targetCell;
                inventory.CellOkupation(targetCell, GetSize(), false);
                inventory.UpdateCellsColor(true);
                return;
            }
        }

        if (prefcell != null)
        {
            SetPosition(this, prefcell);
            inventory.UpdateCellsColor();
            return;
        }

        for (int y = 0; y < inventory.ScalY; y++)
        {
            for (int x = 0; x < inventory.ScalX; x++)
            {
                var cell = inventory.cells[x, y];
                if (inventory.CheckCellFree(cell, GetSize()))
                {
                    SetPosition(this, cell);
                    prefcell = cell;
                    lastInventoryCell = cell;
                    inventory.CellOkupation(cell, GetSize(), false);
                    inventory.UpdateCellsColor(true);
                    return;
                }
            }
        }

        Debug.LogWarning("Нет места в инвентаре! Предмет возвращён в исходную позицию.");
        inventory.UpdateCellsColor();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        );
        rectTransform.anchoredPosition = localPointerPosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragItem = eventData.pointerDrag.GetComponent<Item>();
        if (dragItem == null) return;

        if (dragItem.prefcell != null)
        {
            SetPosition(dragItem, dragItem.prefcell);
            if (inventory != null)
            {
                inventory.CellOkupation(dragItem.prefcell, dragItem.GetSize(), false);
                inventory.UpdateCellsColor();
            }
            else
            {
                Debug.LogError($"Inventory is null in OnDrop for item: {gameObject.name}");
            }
        }
    }

    public void SetPosition(Item item, Cell cell)
    {
        if (!cell)
        {
            return;
        }
        item.transform.SetParent(cell.transform);
        item.transform.localPosition = Vector3.zero;
        var ItemSize = item.GetSize();
        var newPos = item.transform.localPosition;
        if (ItemSize.x == 3)
        {
            newPos.x += 50f;
        }
        if (ItemSize.y == 3)
        {
            newPos.y -= 50f;
        }
        if (ItemSize.x == 2)
        {
            newPos.x += ItemSize.x * 12.5f;
        }
        if (ItemSize.y == 2)
        {
            newPos.y -= ItemSize.y * 12.5f;
        }
        item.transform.localPosition = newPos;
        item.transform.SetParent(canvas.transform);
        if (inventory != null)
        {
            inventory.CellOkupation(cell, ItemSize, false);
            inventory.UpdateCellsColor();
        }
        lastInventoryCell = cell;
    }

    public Vector2Int GetSize()
    {
        Vector2Int size;
        switch (Size)
        {
            case ItemSize.x1y1:
                return size = Vector2Int.one;
            case ItemSize.x2y1:
                return size = new Vector2Int(2, 1);
            case ItemSize.x3y1:
                return size = new Vector2Int(3, 1);
            case ItemSize.x2y2:
                return size = new Vector2Int(2, 2);
            case ItemSize.x1y2:
                return size = new Vector2Int(1, 2);
        }
        return size = Vector2Int.zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RotateItem();
        }
    }

    public void RotateItem()
    {
        var size = GetSize();
        if (size.x == size.y)
        {
            return;
        }
        if (CanRotate())
        {
            FlipSize();
            rectTransform.localEulerAngles += new Vector3(0, 0, -90);
        }
    }

    public bool CanRotate()
    {
        var size = GetSize();
        Vector2Int newSize = new Vector2Int(size.y, size.x);
        return inventory != null && inventory.CheckCellFree(prefcell, newSize);
    }

    public void FlipSize()
    {
        var rect = rectTransform.sizeDelta;
        rectTransform.sizeDelta = new Vector2(rect.y, rect.x);
        switch (Size)
        {
            case ItemSize.x2y1:
                Size = ItemSize.x1y2;
                break;
            case ItemSize.x3y1:
                Size = ItemSize.x1y3;
                break;
            case ItemSize.x1y2:
                Size = ItemSize.x2y1;
                break;
        }
    }
}