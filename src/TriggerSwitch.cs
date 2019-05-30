using UnityEngine;

public class TriggerSwitch : Switch
{
    bool hasTriggered;

    protected void OnTriggerEnter2D(Collider2D other) {
        if (hasTriggered) return;

        if (other.CompareTag("Player")) {
            hasTriggered = true;
            SwitchTriggered();
        }
    }
}
