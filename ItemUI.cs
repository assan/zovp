using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Item item;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;

    [SerializeField] private Image itemImage;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetItem(Item newItem)
    {
        item = newItem;
        if (itemImage != null)
        {
            itemImage.sprite = item.GetComponent<Image>().sprite; // ������������, ��� � Item ���� Image
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        transform.SetParent(transform.root); // ���������� �� ������� ������� Canvas
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // ���������, ���� �������� �������
        GameObject droppedObject = eventData.pointerCurrentRaycast.gameObject;
        Cell targetCell = droppedObject?.GetComponent<Cell>();

        if (targetCell != null)
        {
            Inventory inventory = targetCell.GetComponentInParent<Inventory>();
            if (inventory != null && inventory.CheckCellFree(targetCell, item.GetSize()))
            {
                // ��������� ������� �� ������� � ���������
                Chest chest = transform.GetComponentInParent<Chest>();
                if (chest != null)
                {
                    chest.RemoveItem(item);
                    inventory.AddItemToInventory(item, targetCell);
                    Destroy(gameObject); // ������� UI-������� �� �������
                    return;
                }
            }
        }

        // ���� �� ������� ���������, ���������� ������� �� �����
        transform.SetParent(originalParent);
        transform.position = originalPosition;
    }
}