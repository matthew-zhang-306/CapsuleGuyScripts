using UnityEngine;

// It says "PredictiveLaser" but really this is just any deactivatable laser, it happens to only be used for predictive lines though
public class PredictiveLaser : Laser
{
    public void SetEnabled(bool state) {
        foreach (Transform child in transform)
            child.GetComponent<SpriteRenderer>().enabled = state;
    }
}
