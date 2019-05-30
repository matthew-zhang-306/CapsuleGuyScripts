using UnityEngine;

public class BulletCollision : HazardCollision
{
    protected override void FixedUpdate() {
        base.FixedUpdate();

        // Check for collision with entity
        if (IsColliding)
            EndBullet();
    }

    public void EndBullet() {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
