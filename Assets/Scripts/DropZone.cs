using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DropZone : MonoBehaviour, IDropHandler
{
    public string expectedLetter;
    public TMP_Text slotText;
    public PhonicsGameManager gameManager; // assign in Inspector

    public void OnDrop(PointerEventData eventData)
    {
        DraggablePhonic phonic = eventData.pointerDrag.GetComponent<DraggablePhonic>();
        if (phonic != null)
        {
            // Show the letter in the slot
            slotText.text = phonic.phonicLetter;

            // Play the phoneme sound
            PhonicsAudioManager.instance.PlayLetter(phonic.phonicLetter);

            // Tell game manager to check this letter
            gameManager.CheckLetter(phonic.phonicLetter);

            // ✅ Remove the prefab once it’s used
            Destroy(phonic.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DraggablePhonic cloud = other.GetComponent<DraggablePhonic>();
        if (cloud != null)
        {
            slotText.text = cloud.phonicLetter;

            // Pass to GameManager for validation
            gameManager.CheckLetter(cloud.phonicLetter);
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
