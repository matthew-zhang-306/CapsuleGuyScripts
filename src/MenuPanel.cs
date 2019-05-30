using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public Menu menu;

    public MenuButton firstSelection;
    protected bool hasSetNav;

    protected virtual void Start() {
        SetNav();
    }

    public virtual void SetNav() {
        if (hasSetNav) return;
        hasSetNav = true;

        // By default, menu panels assume that their buttons are laid out vertically one on top of the other and assigns navigation thusly
        MenuButton[] buttons = GetComponentsInChildren<MenuButton>(false);
        for (int b = 0; b < buttons.Length; b++) {
            MenuButton button = buttons[b];
            
            button.Up = buttons[b > 0 ? b-1 : buttons.Length-1];
            button.Down = buttons[b < buttons.Length - 1 ? b+1 : 0];
        }
    }

    public virtual void DidNav(MenuButton selection) {
        // Add logic in derived classes
    }
}
