using UnityEngine;
using UnityEngine.UI;

public class mouseItemController : MonoBehaviour
{
    public Sprite freeItem;
    public static void SetItem(Sprite item)
    {
        mouseItemController MIC = FindObjectOfType<mouseItemController>();
        if (item == null )
        {
            MIC.GetComponent<Image>().sprite = MIC.freeItem;
        }
        else
        {
            MIC.GetComponent<Image>().sprite = item;

        }
    }
    private void Update()
    {
        transform.position = Input.mousePosition;
    }
}
