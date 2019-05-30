using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public List<Controllable> targets;

    public delegate void SwitchAction(Switch s);
    public static event SwitchAction OnPress;
    
    // In derived classes, provide a way for this method to be activated by an outside stimulus
    protected virtual void SwitchTriggered() {
        OnPress?.Invoke(this);
        foreach (Controllable target in targets) {
            if (target != null)
                target.Switch();
            else
                Debug.LogWarning("Switch " + this + " contains a null target!");
        }
    }

    protected virtual void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        foreach (Controllable target in targets)
            if (target != null)
                Gizmos.DrawLine(transform.position, target.transform.position);
    }
}
