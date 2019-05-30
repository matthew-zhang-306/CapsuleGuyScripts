using System.Collections;
using UnityEngine;

// Calling this class "Button" was a mistake because it causes conflicts with Unity's builtin class for UI buttons
public class Button : Switch
{
    SpriteRenderer sr;
    
    public float cooldownTime;
    float cooldownTimer;
    public bool isOneTimePress;
    bool isColliding;
    bool CanPress { get { return cooldownTimer == 0 && !isColliding; }}

    bool inited;

    public GameObject buttonParticles;

    protected void Init() {
        if (inited) return;

        inited = true;
        sr = GetComponent<SpriteRenderer>();
    }
    void Start() {
        Init();
    }

    private void Update() {
        sr.enabled = CanPress;
    }

    protected void OnTriggerEnter2D(Collider2D other) {
        if (!CanPress) return;

        if (other.CompareTag("Laser") || other.CompareTag("PlayerLaser") || other.CompareTag("Player")) {
            // Button has been pressed
            isColliding = true;
            SwitchTriggered();
            
            // Create particle effect
            GameObject particles = Instantiate(buttonParticles, transform.position, transform.rotation, transform);
            ParticleSystem.MainModule particleSettings = particles.GetComponent<ParticleSystem>().main;
            particleSettings.startColor = sr.color;

            if (!isOneTimePress)
                StartCoroutine(DoCooldown());
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        if (isOneTimePress) return;
        if (other.CompareTag("Laser") || other.CompareTag("PlayerLaser") || other.CompareTag("Player")) {
            // Button has been unpressed
            isColliding = false;
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
        Gizmos.color = sr.color;
        foreach (Controllable target in targets)
            if (target != null)
                Gizmos.DrawLine(transform.position, target.transform.position);
    }
}
