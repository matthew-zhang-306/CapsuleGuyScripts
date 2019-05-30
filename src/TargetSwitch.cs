using System.Collections;
using UnityEngine;

public class TargetSwitch : Switch
{
    Animator anim;
    SpriteRenderer sr;

    Sprite offSprite;
    public Sprite onSprite;
    
    public float cooldownTime;
    float cooldownTimer;
    public bool isOneTimeSwitch;
    bool flipped;
    bool CanPress { get { return !(flipped && isOneTimeSwitch) && cooldownTimer == 0; }}

    bool inited;

    [HideInInspector]
    public Vector2 innerScale; // used by the animator: A Vector2 with values between 0 and 1 which indicate the desired size of the target sprite
    Vector2 baseScale;

    public GameObject targetParticlesOff;
    public GameObject targetParticlesOn;

    protected void Init() {
        if (inited) return;

        inited = true;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        offSprite = sr.sprite;

        baseScale = transform.localScale;
        innerScale = Vector2.one;
    }
    void Start() {
        Init();
    }

    void Update() {
        anim.SetBool("Flipped", flipped);
        transform.localScale = new Vector3(baseScale.x * innerScale.x, baseScale.y * innerScale.y, 1);
    }

    protected void OnTriggerEnter2D(Collider2D other) {
        if (!CanPress) return;

        if (other.CompareTag("Laser") || other.CompareTag("PlayerLaser")) {
            SwitchTriggered();
            flipped = !flipped;

            GameObject particles = Instantiate(flipped ? targetParticlesOn : targetParticlesOff, transform.position, Quaternion.identity, transform);

            StartCoroutine(DoCooldown());
        }
    }

    protected IEnumerator DoCooldown() {
        cooldownTimer = cooldownTime;
        while (cooldownTimer > 0) {
            cooldownTimer = Mathf.Max(cooldownTimer - Time.deltaTime, 0);
            yield return 0;
        }
    }

    protected override void OnDrawGizmosSelected() {
        Init();
        Gizmos.color = flipped ? Color.green : Color.red;
        foreach (Controllable target in targets)
            if (target != null)
                Gizmos.DrawLine(transform.position, target.transform.position);
    }
}
