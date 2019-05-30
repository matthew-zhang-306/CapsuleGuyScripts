using UnityEngine;

// It's called "PlayerLaser" but it's really just meant to be a dissolving laser, as the shooting bird also uses it
public class PlayerLaser : Laser
{
    float fireTime;
    float timer;
    Collider2D hitbox;
    Color baseColor;

    public void Init(float fireTime) {
        base.Init();

        this.fireTime = fireTime;
        hitbox = GetComponent<Collider2D>();

        GameObject laserProto = Instantiate(laserSegment, transform.position, Quaternion.identity, transform);
        baseColor = laserProto.GetComponent<SpriteRenderer>().color;
        Destroy(laserProto);
    }

    protected override void FixedUpdate() {
        if (!hasInited) return;

        base.FixedUpdate();

        timer += Time.deltaTime;
        if (timer >= fireTime)
            Destroy(gameObject);
        
        foreach (Transform child in transform) {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            Collider2D hitbox = child.GetComponent<Collider2D>();

            // Don't deal damage when half faded
            if (timer / fireTime >= 0.5)
                hitbox.enabled = false;

            Color c = baseColor;
            c.a = Mathf.Lerp(1, 0, timer / fireTime);
            sr.color = c;
        }
    }
}
