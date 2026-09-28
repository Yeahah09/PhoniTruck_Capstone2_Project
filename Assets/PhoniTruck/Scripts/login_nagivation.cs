using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class login_nagivation : MonoBehaviour
{
   EventSystem system;
   public Selectable first_input;
   public Button submit_btn;

    void Start()
    {
        system = EventSystem.current;
        first_input.Select();
        submit_btn.onClick.AddListener(Login);
    }

    void Update()
    {
      if (Keyboard.current == null)
            return;

        if (Keyboard.current.tabKey.wasPressedThisFrame && Keyboard.current.leftShiftKey.isPressed)
        {
            Selectable current = system.currentSelectedGameObject?.GetComponent<Selectable>();

            if (current != null)
            {
                Selectable prev = current.FindSelectableOnUp();

                if (prev != null)
                {
                    prev.Select();
                }
            }
        }
        else if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Selectable current = system.currentSelectedGameObject?.GetComponent<Selectable>();

            if (current != null)
            {
                Selectable next = current.FindSelectableOnDown();

                if (next != null)
                {
                    next.Select();
                }
            }
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            submit_btn.onClick.Invoke();
        }
    }
          public void Login()
    {
        Debug.Log("System Login!");
    }
}
