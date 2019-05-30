using System.Collections.Generic;
using UnityEngine;

public class HazardCollision : Collision
{
    protected Rigidbody2D rb2d;

    public float damage;
    public bool forceDeath;

    private void Awake() {
        // Remove any tags that belong to the object (aka player cannot hurt himself, enemies cannot hurt themselves)
        List<string> targets = new List<string>{"Player", "Enemy"};
        for (int t = 0; t < targets.Count;) {
            if (this.CompareTag(targets[t]))
                targets.RemoveAt(t);
            else
                t++;
        }
        targetTags = targets.ToArray();
    }

    protected override void OnEnable() {
        base.OnEnable();
        
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
            rb2d = GetComponentInParent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate() {
        if (IsColliding)
            foreach (Collider2D coll in colliders.ToArray()) {
                TryHitEntity(coll.GetComponent<Entity>());
            }
    }

    protected virtual void TryHitEntity(Entity e) {
        if (e == null)
            return;
        
        if (forceDeath)
            e.ForceDeath();
        else
            e.Damage(damage, this); // Hazards damage entities every frame - it is the entity's job to know when they have iframes
    }

    public virtual Vector2 GetVelocity() {
        return rb2d != null ? rb2d.velocity : Vector2.zero;
    }
}
