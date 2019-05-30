using UnityEngine;

public class LaserGun : MonoBehaviour
{   
    Camera mainCamera;

    Transform gunAxis;
    SpriteRenderer sr;
    public SpriteRenderer GunSprite { get { return sr; }}

    public GameObject laserObj;
    PredictiveLaser predictiveLaser;
    
    Vector2 currentAim;
    public Vector2 CurrentAim { get { return currentAim; }}
    
    float distanceFromBarrel;
    Vector2 barrelPosition;

    public bool IsFacingRight { get {
        return Mathf.Cos(Mathf.Deg2Rad * gunAxis.rotation.eulerAngles.z) >= 0;
    }}

    void Start() {
        mainCamera = Camera.main;

        gunAxis = transform.parent;
        sr = GetComponentInChildren<SpriteRenderer>();
        predictiveLaser = GetComponentInChildren<PredictiveLaser>();

        gunAxis.rotation = Quaternion.identity;
        distanceFromBarrel = sr.bounds.max.x - gunAxis.position.x;
    }

    public void Aim() {
        predictiveLaser.SetEnabled(true);

        // AIMING
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition).ToVector2();
        Vector2 gunPosition = gunAxis.position.ToVector2();

        currentAim = (mousePosition - gunPosition).normalized;
        float aimAngle = Vector2.SignedAngle(Vector2.right, currentAim);
        gunAxis.rotation = Quaternion.Euler(0, 0, aimAngle);

        barrelPosition = gunAxis.position.ToVector2() + currentAim * distanceFromBarrel;
    }

    public void Fire(float fireTime) {
        PlayerLaser laser = GameObject.Instantiate(laserObj, barrelPosition, gunAxis.rotation, transform).GetComponent<PlayerLaser>();
        laser.Init(fireTime);

        predictiveLaser.SetEnabled(false);
    }

}
