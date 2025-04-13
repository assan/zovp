using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] Chest chest;
    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            print("E pressed!");
            if (chest != null)
            {
                print("Chest scripts exists!");
                if (!chest.gameObject.activeInHierarchy)
                {
                    print("Chest is to be closed!");
                    chest.CloseChest();
                }
                else
                {
                    print("Chest is to be opened!");
                    chest.OpenChest();
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            print("Player is in range!");
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            print("Player out of range!");
            isPlayerInRange = false;
            if (chest != null && chest.gameObject.activeInHierarchy)
            {
                chest.CloseChest();
            }
        }
    }
}