using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Rect room;
    Rect panningSpace;

    public bool drawStaticRegionGizmos;

    Transform player;
    Vector2 desiredPosition;
    public Vector2 followOffset;
    float zOffset;

    Vector2 lastStaticRegion;
    int staticRegionLayerMask;
    float towardsStaticRegion;
    public float staticRegionLerpTime;

    public float screenShakeIntensity;
    public float screenShakeTime;
    float screenShakeBaseIntensity;
    float screenShakeTimer;

    private void OnEnable() {
        PlayerController.OnFire += DoPlayerFireScreenShake;
        PlayerController.OnHit += DoPlayerHitScreenShake;
        PlayerController.OnPlayerDeath += DoPlayerDeathScreenShake;
        ShootingBirdEnemy.OnShoot += DoBirdShootScreenShake;
    }
    private void OnDisable() {
        PlayerController.OnFire -= DoPlayerFireScreenShake;
        PlayerController.OnHit -= DoPlayerHitScreenShake;
        PlayerController.OnPlayerDeath -= DoPlayerDeathScreenShake;
        ShootingBirdEnemy.OnShoot -= DoBirdShootScreenShake;
    }

    // SCREENSHAKE METHODS
    void DoPlayerFireScreenShake()                  => DoScreenShake(1.5f,1.5f);
    void DoPlayerHitScreenShake(Entity player)      => DoScreenShake(1,1);
    void DoPlayerDeathScreenShake()                 => DoScreenShake(4,2);
    void DoBirdShootScreenShake(BirdEnemy bird)     => DoScreenShake(1,1);

    void Start() {
        Vector2 cameraSize = GetComponent<Camera>().ViewportSize();

        panningSpace = room;
        Vector2 roomCenter = panningSpace.center; // store the previous center of the rectangle
        panningSpace.size = VectorExtensions.Max(panningSpace.size, cameraSize);
        panningSpace.size -= cameraSize;
        panningSpace.center = roomCenter;         // reset the center after scaling

        player = GameObject.FindGameObjectWithTag("Player").transform;
        zOffset = transform.position.z;

        staticRegionLayerMask = LayerMask.GetMask("CameraTrigger");
    }

    void FixedUpdate() {
        if (player != null)
            desiredPosition = VectorExtensions.ClampInRect(player.position.ToVector2() + followOffset, panningSpace);

        // Find static region
        Vector2 staticRegionPosition = lastStaticRegion;
        Collider2D staticRegion = Physics2D.OverlapBox(desiredPosition, new Vector2(0.1f, 0.1f), 0f, staticRegionLayerMask);
        if (staticRegion != null) {
            staticRegionPosition = staticRegion.transform.position;
            lastStaticRegion = staticRegionPosition;
        }

        // Apply static region
        float staticRegionDir = (staticRegion != null ? 1 : -1) * Time.deltaTime / staticRegionLerpTime;
        towardsStaticRegion = Mathf.Clamp(towardsStaticRegion + staticRegionDir, 0, 1);

        transform.position = Vector2.Lerp(desiredPosition, staticRegionPosition, Easing.QuadInOut(towardsStaticRegion));
        transform.position += Vector3.forward * zOffset;

        // Apply screen shake
        screenShakeTimer = Mathf.Max(screenShakeTimer - Time.deltaTime, 0);
        Vector3 screenShakeOffset = Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector3.right;
        transform.position += screenShakeOffset * screenShakeBaseIntensity * (screenShakeTimer / screenShakeTime);
    }

    void DoScreenShake(float timeMult, float intensityMult) {
        screenShakeTimer = screenShakeTime * timeMult;
        screenShakeBaseIntensity = screenShakeIntensity * intensityMult;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(room.center, room.size);

        if (panningSpace != null) {
            Gizmos.color = new Color(0.5f, 1, 1, 0.25f);
            Gizmos.DrawWireCube(panningSpace.center, panningSpace.size);
        }
    }
}
