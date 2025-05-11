using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    private Inventory inventory;
    private bool isInventoryOpen = false;
    private bool isOpenedByChest = false; // Флаг, чтобы отслеживать, открыт ли инвентарь сундуком
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    private AudioSource audioSource;

    void Start()
    {
        inventory = inventoryPanel.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory component not found on InventoryPanel!");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }
        else
        {
            Debug.LogError("InventoryPanel is not assigned in InventoryManager!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !isOpenedByChest)
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
            if (Player.Instance != null)
            {
                Player.Instance.Initialize();
            }
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
        }
        else
        {
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

    public void OpenInventoryForChest()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = true;
        isOpenedByChest = true;
        inventoryPanel.SetActive(true);

        if (inventory != null)
        {
            inventory.InitializeInventory();
        }
        if (Player.Instance != null)
        {
            Player.Instance.Initialize();
        }
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        Debug.Log("Inventory opened for chest.");
    }

    public void CloseInventoryForChest()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = false;
        isOpenedByChest = false;
        inventoryPanel.SetActive(false);

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

        Debug.Log("Inventory closed for chest.");
    }
}