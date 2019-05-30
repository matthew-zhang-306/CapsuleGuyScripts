using UnityEngine;

public class SwitchPlatform : Controllable
{
    public bool startActivated;
    public bool isOneWay;

    SpriteRenderer sr;
    Collider2D coll;

    public SpriteRenderer dottedTexture;

    public GameObject particles;
    
    private void Start() {
        sr = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        
        // Init dotted texture scale + color
        if (dottedTexture != null) {
            dottedTexture.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1);
            dottedTexture.size = new Vector2(transform.localScale.x, transform.localScale.y);
            dottedTexture.color = GetColorWithAlpha(0.4f);
        }

        activated = true;

        if (!startActivated) {
            // if the object is one way, calling Switch() will not work to initially deactivate the platform, so we temporarily set the object as NOT one way to do that
            bool tempIsOneWay = isOneWay;
            isOneWay = false;
            Switch();
            isOneWay = tempIsOneWay;
        }
    }

    public override void Switch() {
        if (activated && isOneWay) return;
        activated = !activated;

        coll.enabled = activated;
        
        sr.color = GetColorWithAlpha(activated ? 1f : 0.1f);
        
        // Hard-coded sorting layer management
        sr.sortingLayerName = activated ? "Foreground" : "MovingPlatformTrack";
        if (dottedTexture != null)
            dottedTexture.sortingLayerName = activated ? "Foreground" : "MovingPlatformTrack";
    
        // Create particle effect
        if (activated && particles != null) {
            ParticleSystem theParticles = Instantiate(particles, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ParticleSystem.MainModule mainSettings = theParticles.main;
            mainSettings.startColor = sr.color;
            ParticleSystem.ShapeModule shapeSettings = theParticles.shape;
            shapeSettings.scale = new Vector3(transform.localScale.x / 2, transform.localScale.y / 2, 1);
        }
    }

    Color GetColorWithAlpha(float alpha) {
        Color c = sr.color;
        c.a = alpha;
        return c;
    }
}
