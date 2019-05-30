using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    protected EventSystem eventSystem;

    public GameObject globalPanelObj;
    protected List<MenuPanel> panels;
    protected MenuPanel currentPanel;

    protected List<GameObject> buttons;
    protected MenuButton currentSelection;
    protected MenuButton previousSelection;
    protected bool isMouseOver;

    protected float vAxis;
    protected float oldVAxis;
    protected float hAxis;
    protected float oldHAxis;
    protected bool HasNavInput { get { return ((oldVAxis != vAxis && vAxis != 0) || (oldHAxis != hAxis && hAxis != 0)) && !isMouseOver; }}

    protected bool navigationDisabled;

    protected void OnEnable() {
        MenuButton.OnHover += OnHover;
        MenuButton.OnUnhover += OnUnhover;
    }
    protected void OnDisable() {
        MenuButton.OnHover -= OnHover;
        MenuButton.OnUnhover -= OnUnhover;
    }

    protected virtual void Start() {
        eventSystem = EventSystemSingleton.Instance;

        panels = new List<MenuPanel>();
        foreach (Transform child in transform) {
            MenuPanel panel = child.GetComponent<MenuPanel>();
            if (panel != null) {
                panel.menu = this;
                panels.Add(panel);
                if (panel.gameObject.activeSelf) {
                    currentPanel = panel;
                    InitButtons();    
                }
            }
        }
    }

    protected virtual void Update() {
        oldVAxis = vAxis;
        vAxis = Input.GetAxisRaw("Vertical");
        oldHAxis = hAxis;
        hAxis = Input.GetAxisRaw("Horizontal");

        if (navigationDisabled) return;
        if (currentPanel == null) return;
        
        if (HasNavInput) {
            if (currentSelection != null)
                currentSelection = currentSelection.GetNav(hAxis, vAxis); // If there is a current selection, use its pre-established navigation logic
            else if (previousSelection != null)
                currentSelection = previousSelection; // Or if there was a previus selection, select that
            else
                currentSelection = currentPanel.firstSelection; // Otherwise, select a default option known by the current menu panel

            eventSystem.SetSelectedGameObject(currentSelection.gameObject);
            currentPanel.DidNav(currentSelection);
        }
    }

    protected virtual void OnHover(PointerEventData eventData) {
        isMouseOver = true;
        currentSelection = eventData.selectedObject.GetComponent<MenuButton>();
        eventSystem.SetSelectedGameObject(eventData.selectedObject);
    }
    protected virtual void OnUnhover(PointerEventData eventData) {
        isMouseOver = false;
        previousSelection = currentSelection;
        currentSelection = null;
        eventSystem.SetSelectedGameObject(null);
    }

    public void OpenPanel(string name) {
        if (navigationDisabled) return;
        
        currentPanel = null;
        foreach (MenuPanel panel in panels) {
            // Activate (and note) the desired panel and deactivate all others
            if (panel.name.ToLower().Contains(name.ToLower())) {
                currentPanel = panel;
                panel.gameObject.SetActive(true);
            } else {
                panel.gameObject.SetActive(false);
            }
        }
        globalPanelObj.SetActive(currentPanel != null);

        if (currentPanel == null && name != "close") {
            Debug.LogError("There is no menu panel with a name containing " + name + "!");
        }

        InitButtons();
    }

    protected void InitButtons() {
        if (currentPanel != null) {
            currentSelection = null;
            previousSelection = currentPanel.firstSelection;
            isMouseOver = false;
        }
    }

    // Mostly a safekeeping method to ensure that the menu understands which button should be selected first
    public void SetPreviousSelection(MenuButton selection) {
        previousSelection = selection;
    }
}
