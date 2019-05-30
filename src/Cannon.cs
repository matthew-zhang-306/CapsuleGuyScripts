using UnityEngine;

public class Cannon : MonoBehaviour
{
    public GameObject bullet;
    
    Transform cannonaxis;
    Transform cannonhead;
    SpriteRenderer cannonheadSprite;
    float distanceFromTip;
    Vector2 tipOfCannon { get { return cannonhead.position + distanceFromTip * cannonhead.right; }}

    public float bulletSpeed;
    public float fireTime;
    public float cycleOffset;
    float fireTimer;

    private void Start() {
        cannonaxis = transform.GetChild(0);
        cannonhead = cannonaxis.GetChild(0);
        cannonheadSprite = cannonhead.GetComponent<SpriteRenderer>();
        
        Quaternion rot = transform.rotation;
        transform.rotation = Quaternion.identity;
        distanceFromTip = cannonheadSprite.bounds.max.x - cannonhead.position.x; // calculate the head distance with rotation at 0
        transform.rotation = rot;
        
        fireTimer += fireTime - cycleOffset;
    }

    private void FixedUpdate() {
        fireTimer += Time.deltaTime;
        if (fireTimer > fireTime) {
            fireTimer -= fireTime;

            // Create bullet
            GameObject theBullet = Instantiate(bullet, tipOfCannon, cannonhead.rotation);
            theBullet.GetComponent<Bullet>().Init(bulletSpeed, this);
        }
    }


}
