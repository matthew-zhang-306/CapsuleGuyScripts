using System.Linq;
using UnityEngine;

public class AudioTrigger : Collision
{
    public string paletteName;
    public string transitionName;

    protected override void OnTriggerEnter2D(Collider2D other) {
        if (targetTags.Any(tag => other.CompareTag(tag))) {
            colliders.Insert(0, other);
            AudioManager.Instance?.TransitionToNewPalette(paletteName, transitionName);
        }
    }
}
