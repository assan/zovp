using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemType itemType;
    public ItemSize itemSize;
    public float defenseBonus;
    public float attackBonus;

    public Cell prefcell;
    public Cell lastInventoryCell;
    public Inventory inventory;
    public CanvasGroup canvasGroup;

    private Chest currentChest; // Ссылка на сундук, если предмет находится в сундуке
    private Transform originalParent;
    private Vector3 originalPosition;
    public Canvas canvas;
    private RectTransform rectTransform;
    public Item prefabReference; // Ссылка на префаб этого предмета

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"Canvas not found for item: {gameObject.name}");
        }

        inventory = Inventory.Instance;
        if (inventory == null)
        {
            Debug.LogError($"Inventory not found for item: {gameObject.name}");
        }
        Debug.Log($"Item {gameObject.name} created with parent: {(transform.parent != null ? transform.parent.name : "null")}");
    }

    // Устанавливаем ссылку на сундук
    public void SetChest(Chest chest)
    {
        currentChest = chest;
        inventory = currentChest == null ? Inventory.Instance : null; // Если предмет в сундуке, отключаем ссылку на инвентарь

        // Меняем внешний вид, чтобы визуально различать предметы в сундуке
        Image itemImage = GetComponent<Image>();
        if (itemImage != null)
        {
            itemImage.color = currentChest != null ? new Color(1f, 0.8f, 0.8f) : Color.white; // Лёгкий красный оттенок в сундуке
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mouseItemController.SetItem(GetComponent<Image>().sprite); //создаём клон изображения на mouseItem
        Debug.Log($"OnBeginDrag called for item: {gameObject.name}, prefcell: {(prefcell != null ? prefcell.gameObject.name : "null")}, lastInventoryCell: {(lastInventoryCell != null ? lastInventoryCell.gameObject.name : "null")}");

        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        originalParent = transform.parent;
        originalPosition = transform.position;
        // transform.SetParent(canvas.transform,false); // Перемещаем на верхний уровень Canvas
        transform.parent = canvas.transform;
        // Если предмет был в инвентаре
        if (currentChest == null)
        {
            inventory = Inventory.Instance;
            if (inventory == null || !inventory.gameObject.activeInHierarchy)
            {
                Debug.LogError($"Inventory is null or not active in OnBeginDrag for item: {gameObject.name}");
                return;
            }

            inventory.DragedItem = this;

            EquipmentSlot currentSlot = originalParent.GetComponent<EquipmentSlot>();
            if (currentSlot != null)
            {
                Debug.Log($"Item was in EquipmentSlot: {currentSlot.gameObject.name}");
                currentSlot.equippedItem = null;
                if (lastInventoryCell != null)
                {
                    Debug.Log($"Freeing cells starting from lastInventoryCell: {lastInventoryCell.gameObject.name}");
                    inventory.CellOkupation(lastInventoryCell, GetSize(), true);
                }

                if (Player.Instance != null && Player.Instance.gameObject.activeInHierarchy)
                {
                    Debug.Log($"Updating player stats after removing item {gameObject.name} from slot {currentSlot.gameObject.name}");
                    Player.Instance.UpdateStats();
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Time.timeScale == 0) return;
        //rectTransform.anchoredPosition = eventData.position / canvas.scaleFactor;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        GameObject droppedObject = eventData.pointerCurrentRaycast.gameObject;
        Cell targetCell = null;
        EquipmentSlot targetSlot = null;
        Transform chestContainer = null;

        if (droppedObject != null)
        {
            targetCell = droppedObject.GetComponent<Cell>();
            targetSlot = droppedObject.GetComponent<EquipmentSlot>();
            chestContainer = droppedObject.transform.parent; // Проверяем, сброшен ли предмет в контейнер сундука
        }

        // Если предмет сброшен в ячейку инвентаря
  
        if (targetCell != null)
        {
            Inventory targetInventory = targetCell.GetComponentInParent<Inventory>();
            if (targetInventory != null && targetInventory.CheckCellFree(targetCell, GetSize()))
            {
                Debug.Log($"Attempting to transfer item {gameObject.name} from chest to inventory at cell ({targetCell.x}, {targetCell.y})");

                // Сначала добавляем предмет в инвентарь
                inventory = targetInventory;
                inventory.DragedItem = null;
                inventory.AddItemToInventory(this, targetCell);

                // Затем удаляем его из сундука
                if (currentChest != null)
                {
                    currentChest.RemoveItem(this);
                    currentChest = null;
                }

                Debug.Log($"Successfully transferred item {gameObject.name} to inventory.");
                return;
            }
            else
            {
                Debug.LogWarning($"Cannot transfer item {gameObject.name} to inventory: cell ({targetCell.x}, {targetCell.y}) is not free.");
            }
            Debug.Log($"OnEndDrag finished for item {gameObject.name}. Current parent: {(transform.parent != null ? transform.parent.name : "null")}");
        }

        // Если предмет сброшен в слот экипировки
        if (targetSlot != null && targetSlot.slotType == itemType)
        {
            if (currentChest != null)
            {
                currentChest.RemoveItem(this);
                currentChest = null;
            }

            inventory = Inventory.Instance;
            if (inventory == null) return;

            inventory.DragedItem = null;

            if (targetSlot.equippedItem != null)
            {
                targetSlot.ReturnItemToInventory(targetSlot.equippedItem);
            }
            targetSlot.equippedItem = this;
            transform.SetParent(targetSlot.transform);
            transform.localPosition = Vector3.zero;
            if (prefcell != null)
            {
                lastInventoryCell = prefcell;
            }
            prefcell = null;
            if (Player.Instance != null && Player.Instance.gameObject.activeInHierarchy)
            {
                Player.Instance.UpdateStats();
            }
            inventory.UpdateCellsColor();
            return;
        }

        // Если предмет сброшен в контейнер сундука
        if (chestContainer != null)
        {
            Chest chest = chestContainer.GetComponentInParent<Chest>();
            if (chest != null)
            {
                // Если предмет был в инвентаре, освобождаем ячейки
                if (inventory != null)
                {
                    if (prefcell != null)
                    {
                        inventory.CellOkupation(prefcell, GetSize(), true);
                    }
                    else if (lastInventoryCell != null)
                    {
                        inventory.CellOkupation(lastInventoryCell, GetSize(), true);
                    }
                    inventory.DragedItem = null;
                    inventory.UpdateCellsColor();
                }

                // Добавляем предмет в сундук
                inventory = null;
                prefcell = null;
                lastInventoryCell = null;
                currentChest = chest;
                chest.AddItem(this);
                return;
            }
        }

        // Если сброшен за пределы инвентаря, слотов или сундука — выбрасываем предмет
        if (targetCell == null && targetSlot == null && chestContainer == null)
        {
            Debug.Log($"Item {gameObject.name} dropped outside inventory, equipment slots, or chest. Destroying item.");
            if (currentChest != null)
            {
                currentChest.RemoveItem(this);
                currentChest = null;
            }
            else if (inventory != null)
            {
                if (prefcell != null)
                {
                    inventory.CellOkupation(prefcell, GetSize(), true);
                }
                else if (lastInventoryCell != null)
                {
                    inventory.CellOkupation(lastInventoryCell, GetSize(), true);
                }
                inventory.DragedItem = null;
                inventory.UpdateCellsColor();
            }
            Destroy(gameObject);
            return;
        }

        // Если не удалось разместить предмет, возвращаем его на место
        if (currentChest != null)
        {
            transform.SetParent(originalParent);
            transform.position = originalPosition;
            return;
        }

        if (inventory == null) inventory = Inventory.Instance;
        if (prefcell != null)
        {
            SetPosition(this, prefcell);
            inventory.UpdateCellsColor();
            inventory.DragedItem = null;
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
                    inventory.DragedItem = null;
                    return;
                }
            }
        }

        Debug.LogWarning("Нет места в инвентаре! Предмет возвращён в исходную позицию.");
        inventory.UpdateCellsColor();
        inventory.DragedItem = null;

        // Добавляем отладку, чтобы видеть, где находится предмет после сброса
        Debug.Log($"Item {gameObject.name} dropped. Current location: {(currentChest != null ? "Chest" : inventory != null ? "Inventory" : "Dropped")}");
    }

    public void SetPosition(Item item, Cell cell)
    {
        transform.SetParent(cell.transform.parent);
        RectTransform itemRect = GetComponent<RectTransform>();
        itemRect.anchoredPosition = cell.GetComponent<RectTransform>().anchoredPosition;
        itemRect.localScale = Vector3.one;
        itemRect.sizeDelta = new Vector2(32, 32);
        CanvasGroup itemCanvasGroup = GetComponent<CanvasGroup>();
        if (itemCanvasGroup != null)
        {
            itemCanvasGroup.alpha = 1f;
            itemCanvasGroup.blocksRaycasts = true;
        }
    }

    public Vector2Int GetSize()
    {
        Vector2Int size;
        switch (itemSize)
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
            case ItemSize.x1y3: // Добавляем поддержку нового размера после поворота
                return size = new Vector2Int(1, 3);
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
        switch (itemSize)
        {
            case ItemSize.x2y1:
                itemSize = ItemSize.x1y2;
                break;
            case ItemSize.x3y1:
                itemSize = ItemSize.x1y3;
                break;
            case ItemSize.x1y2:
                itemSize = ItemSize.x2y1;
                break;
            case ItemSize.x1y3:
                itemSize = ItemSize.x3y1;
                break;
        }
    }
    void OnDestroy()
    {
        Debug.Log($"Item {gameObject.name} was destroyed.");
    }
}