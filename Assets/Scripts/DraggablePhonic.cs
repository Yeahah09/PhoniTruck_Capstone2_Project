using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggablePhonic : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public string phonicLetter;
    public TMP_Text letterText; // assign in prefab Inspector

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector2 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.anchoredPosition;
        UpdateLetterText();
    }

    public void SetLetter(string letter)
    {
        phonicLetter = letter;
        UpdateLetterText();
    }

    private void UpdateLetterText()
    {
        if (letterText != null)
            letterText.text = phonicLetter;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // If not dropped in a DropZone, snap back
        if (transform.parent.GetComponent<DropZone>() == null)
        {
            rectTransform.anchoredPosition = startPosition;
            transform.SetParent(originalParent);
        }
    }



    // 🔊 Play sound when clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(phonicLetter))
        {
            PhonicsAudioManager.instance.PlayLetter(phonicLetter);
        }
    }
}
