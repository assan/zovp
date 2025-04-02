using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel; // Ссылка на объект инвентаря
    private Inventory inventory; // Ссылка на компонент Inventory
    private bool isInventoryOpen = false;
    [SerializeField] private AudioClip openSound; // Звук открытия инвентаря
    [SerializeField] private AudioClip closeSound; // Звук закрытия инвентаря
    private AudioSource audioSource;

    void Start()
    {
        // Находим компонент Inventory
        inventory = inventoryPanel.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory component not found on InventoryPanel!");
        }

        // Убедись, что инвентарь изначально выключен
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }
        else
        {
            Debug.LogError("InventoryPanel is not assigned in InventoryManager!");
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Проверяем нажатие клавиши Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            if (inventory != null)
            {
                inventory.InitializeInventory();
            }
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
        }
        else
        {
            // Сбрасываем перетаскивание, если инвентарь закрывается
            if (inventory != null && inventory.DragedItem != null)
            {
                Item draggedItem = inventory.DragedItem;
                draggedItem.canvasGroup.alpha = 1f;
                draggedItem.canvasGroup.blocksRaycasts = true;
                if (draggedItem.prefcell != null)
                {
                    draggedItem.SetPosition(draggedItem, draggedItem.prefcell);
                }
                inventory.DragedItem = null;
            }

            if (audioSource != null && closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
            }
        }

        Debug.Log($"Inventory is now {(isInventoryOpen ? "open" : "closed")}");
    }
}