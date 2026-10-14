using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DropZone : MonoBehaviour, IDropHandler
{
    public TMP_Text slotText;

    public void OnDrop(PointerEventData eventData)
    {
        DraggablePhonic phonic = eventData.pointerDrag.GetComponent<DraggablePhonic>();
        if (phonic != null)
        {
            phonic.transform.SetParent(transform);
            phonic.transform.localPosition = Vector3.zero;
            slotText.text = phonic.phonicLetter;
        }
    }

    public void ClearSlot()
    {
        slotText.text = "";
        slotText.color = Color.black;
    }

    public void HighlightCorrect()
    {
        slotText.color = Color.green;
    }
}
