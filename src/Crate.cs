using System.Collections;
using UnityEngine;

public class Crate : Entity
{
    public Sprite crackedSprite;
    public GameObject groundObject;
    public float deathDelay;

    public override bool Damage(float h, HazardCollision coll) {
        if (!base.Damage(h, coll))
            return false;

        if (health > 0)
            sr.sprite = crackedSprite;
        
        return true;
    }

    protected override IEnumerator DoHit() {
        yield return new WaitForSeconds(deathDelay);
        SetSpriteMaterial(spriteMat);
        yield return new WaitForSeconds(hitTime - deathDelay);
        EndHit();
    }

    // Object turns white and then dies shortly after
    protected override void Die() {
        Destroy(groundObject);
        SetSpriteMaterial(whiteMat);
        StartIFrames();
        StartCoroutine(DieWithDelay());
    }
    IEnumerator DieWithDelay() {
        yield return new WaitForSeconds(deathDelay);
        base.Die();
    }
}
