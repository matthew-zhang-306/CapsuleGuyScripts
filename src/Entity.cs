using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
    [HideInInspector]
    public int enemyGroup;

    public float maxHealth;
    protected float health;

    public float iFrameTime;
    protected float iFrameTimer;
    public float hitTime;

    protected bool IsHit { get {
        return iFrameTimer > 0;
    }}

    protected Rigidbody2D rb2d;
    protected Collider2D coll;
    protected List<SpriteRenderer> allSr;
    protected SpriteRenderer sr;

    protected Material spriteMat;
    protected Material whiteMat;

    public delegate void EntityAction(Entity e);
    public static event EntityAction OnHit;
    public static event EntityAction OnKilled;

    public GameObject deathParticles;

    protected virtual void Start() {
        health = maxHealth;
        rb2d = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        
        allSr = new List<SpriteRenderer>();
        sr = GetComponent<SpriteRenderer>(); // Contains either the attached component or the first component of a child
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
        else
            allSr.Add(sr);
        
        foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>())
            allSr.Add(s);

        spriteMat = sr.material;
        whiteMat = Resources.Load<Material>("White");
    }

    protected virtual void Update() {
        // Kill the entity if they are clipped far into a wall
        Collider2D g = Physics2D.OverlapBox(transform.position, coll.bounds.size / 3, 0f, LayerMask.GetMask("Wall"));
        if (g != null && g.gameObject.GetComponentInParent<Entity>() != this) {
            Die();
        }
    }

    // return value of this function is whether or not the damage was actually dealt
    public virtual bool Damage(float h, HazardCollision hitColl) {
        if (IsHit)
            return false;
        
        health -= h;
        if (health <= 0)
            Die();
        else {
            StartHit();
            StartIFrames();
        }
        
        return true;
    }

    protected virtual void StartIFrames() {
        StartCoroutine(DoIFrames());
    }
    protected virtual IEnumerator DoIFrames() {
        iFrameTimer = iFrameTime;
        while (iFrameTimer > 0) {
            yield return 0;
            iFrameTimer = Mathf.Max(iFrameTimer - Time.deltaTime, 0);
        }
        EndIFrames();
    }
    protected virtual void EndIFrames() {
        // Add logic here in derived classes
    }

    protected virtual void StartHit() {
        OnHit?.Invoke(this);
        SetSpriteMaterial(whiteMat);
        StartCoroutine(DoHit());
    }
    protected virtual IEnumerator DoHit() {
        yield return new WaitForSeconds(hitTime);
        EndHit();
    }
    protected virtual void EndHit() {
        SetSpriteMaterial(spriteMat);
    }

    protected virtual void SetSpriteMaterial(Material m) {
        foreach (SpriteRenderer s in allSr)
            if (s != null)
                s.material = m;
    }

    public virtual void ForceDeath() {
        health = 0;
        Die();
    }
    protected virtual void Die() {
        if (deathParticles != null)
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        OnKilled?.Invoke(this);
        Destroy(gameObject);
    }
}
