using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel; // ������ �� ������ ���������
    private Inventory inventory; // ������ �� ��������� Inventory
    private bool isInventoryOpen = false;
    [SerializeField] private AudioClip openSound; // ���� �������� ���������
    [SerializeField] private AudioClip closeSound; // ���� �������� ���������
    private AudioSource audioSource;

    void Start()
    {
        // ������� ��������� Inventory
        inventory = inventoryPanel.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory component not found on InventoryPanel!");
        }

        // �������, ��� ��������� ���������� ��������
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
        // ��������� ������� ������� Tab
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
            // ���������� ��������������, ���� ��������� �����������
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