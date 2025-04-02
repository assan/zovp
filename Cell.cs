using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Armory armory;
    public Inventory inventory;
    public TMP_Text cellIndex;
    public int x, y;
    public bool isFree;
    public Image image;

    void Start()
    {
        image = GetComponent<Image>();
        // Пытаемся инициализировать inventory через Singleton, если он ещё не установлен
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
            if (inventory == null)
            {
                Debug.LogError($"Inventory not found for cell: {gameObject.name}");
            }
        }

        // Пытаемся инициализировать armory через Singleton или оставляем null
        if (armory == null)
        {
            armory = FindObjectOfType<Armory>(); // Лучше заменить на Armory.Instance, если добавишь Singleton
            if (armory == null)
            {
                Debug.LogWarning($"Armory not found for cell: {gameObject.name}");
            }
        }
    }

    void Update()
    {
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragItem = eventData.pointerDrag.GetComponent<Item>();
        if (dragItem == null)
        {
            Debug.LogWarning($"No Item component found on dragged object in cell: {gameObject.name}");
            return;
        }

        if (inventory == null)
        {
            Debug.LogError($"Inventory is null in OnDrop for cell: {gameObject.name}");
            return;
        }

        Debug.Log($"Dropping item {dragItem.gameObject.name} on cell ({x}, {y})");
        // Размещение теперь обрабатывается в Item.OnEndDrag
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventory == null)
        {
            Debug.LogError($"Inventory is null in OnPointerEnter for cell: {gameObject.name}");
            return;
        }

        if (inventory.DragedItem)
        {
            bool isFree = inventory.CheckCellFree(this, inventory.DragedItem.GetSize());
            Debug.Log($"Pointer entered cell ({x}, {y}): isFree = {isFree}");
            if (isFree)
            {
                inventory.Coloring(this, inventory.DragedItem.GetSize(), Color.green);
            }
            else
            {
                inventory.Coloring(this, inventory.DragedItem.GetSize(), Color.red);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventory == null)
        {
            Debug.LogError($"Inventory is null in OnPointerExit for cell: {gameObject.name}");
            return;
        }

        if (inventory.DragedItem)
        {
            Debug.Log($"Pointer exited cell ({x}, {y})");
            inventory.Coloring(this, inventory.DragedItem.GetSize(), Color.black);
        }
    }

    public void Initialized(Inventory inv, Armory armory = null)
    {
        inventory = inv;
        if (inventory == null)
        {
            Debug.LogError($"Inventory passed to Initialized is null for cell: {gameObject.name}");
        }

        this.armory = armory;
        if (this.armory == null)
        {
            this.armory = FindObjectOfType<Armory>(); // Лучше заменить на Armory.Instance
            if (this.armory == null)
            {
                Debug.LogWarning($"Armory not found for cell: {gameObject.name}");
            }
        }
    }
}