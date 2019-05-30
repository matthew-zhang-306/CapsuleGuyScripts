using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public delegate void HoverAction(PointerEventData eventData);
    public static event HoverAction OnHover;
    public static event HoverAction OnUnhover; 
    public static event HoverAction OnClick;

    // I learned too late that Unity's buttons already have these variables built-in
    public MenuButton Left;
    public MenuButton Right;
    public MenuButton Down;
    public MenuButton Up;

    public void OnPointerEnter(PointerEventData eventData) {
        eventData.selectedObject = gameObject;
        if (OnHover != null)
            OnHover(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) {
        eventData.selectedObject = gameObject;
        if (OnUnhover != null)
            OnUnhover(eventData);
    }

    public void OnPointerClick(PointerEventData eventData) {
        eventData.selectedObject = gameObject;
        if (OnClick != null)
            OnClick(eventData);
    }

    public MenuButton GetNav(float xInput, float yInput) {
        MenuButton desiredButton = null;
        if (yInput < 0)
            desiredButton = Down;
        if (yInput > 0)
            desiredButton = Up;
        if (xInput < 0)
            desiredButton = Left;
        if (xInput > 0)
            desiredButton = Right;
        
        return desiredButton != null ? desiredButton : this;
    }
}
