using UnityEngine;

public class PlayerLaserCollision : HazardCollision
{
    protected override void FixedUpdate() {
        if (IsColliding)
            foreach (Collider2D coll in colliders.ToArray()) {
                Entity e = coll.GetComponent<Entity>();
                if (e == null)
                    continue;
                
                // The laser should not hit any enemies when way offscreen
                EnableByCamera en = coll.GetComponent<EnableByCamera>();
                if (en != null && !en.IsOn)
                    continue;

                e.Damage(damage, this);
            }
    }

    public override Vector2 GetVelocity() {
        return transform.right;
    }
    
}
