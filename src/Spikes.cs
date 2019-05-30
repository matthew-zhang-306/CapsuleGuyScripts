using UnityEngine;

public class Spikes : HazardCollision {

    protected override void TryHitEntity(Entity e) {
        if (e == null)
            return;
        if (!forceDeath && (e.GetType() == typeof(GroundEntity) || e.GetType().IsSubclassOf(typeof(GroundEntity)))) {
            GroundEntity g = e as GroundEntity;

            // If the entity is moving away from the spikes, do not hit them
            if (Vector2.Dot(g.RealVelocity, transform.up) > 0.5f)
                return;
        }

        base.TryHitEntity(e);
    }

}