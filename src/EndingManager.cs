using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndingManager : MonoBehaviour
{
    Animator anim;

    public TextMeshProUGUI[] endTexts;
    public BackgroundLayer starsBG;
    List<SpriteRenderer> starsSRs;
    public float approachEndZone;

    PlayerController player { get { return GameManager.Instance?.Player; }}

    private void Start() {
        anim = GetComponent<Animator>();

        starsSRs = new List<SpriteRenderer>();
        foreach (SpriteRenderer sr in starsBG.GetComponentsInChildren<SpriteRenderer>()) {
            starsSRs.Add(sr);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            anim.SetBool("Ending", true);
            if (player != null) {
                player.StartLevelTransition(1);
                GameManager.Instance?.HitEnd();
            }
        }
    }
    
    private void FixedUpdate() {
        // Gets a float from 0 to 1 representing the player's horizontal progress through the end zone
        float playerPos = 0f;
        if (player != null)
            playerPos = Mathf.Clamp((player.transform.position.x - (transform.position.x - approachEndZone)) / approachEndZone, 0, 1);

        AudioManager.Instance?.SetParameter("Song4End", playerPos);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector2(transform.position.x - approachEndZone / 2, transform.position.y), new Vector3(approachEndZone, 1, 1));
    }


    public void DoTextFadeIn(float time) {
        StartCoroutine(TextFadeIn(time));
    }
    IEnumerator TextFadeIn(float time) {
        float timer = 0f;
        while (timer < time) {
            yield return 0;
            timer += Time.deltaTime;
            foreach (TextMeshProUGUI endText in endTexts)
                endText.color = new Color(endText.color.r, endText.color.g, endText.color.b, timer / time);
        }
    }

    public void DoStarsFadeIn(float time) {
        StartCoroutine(StarsFadeIn(time));
    }
    IEnumerator StarsFadeIn(float time) {
        float timer = 0f;
        while (timer < time) {
            yield return 0;
            timer += Time.deltaTime;
            foreach (SpriteRenderer sr in starsSRs)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, timer / time);
        }
    }

    public void DoMenuFadeOut() {
        GameManager.Instance?.BackToMenu();
    }
}
