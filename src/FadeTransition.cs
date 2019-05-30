using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    Image image;
    Color baseColor;

    private void Start() {
        image = GetComponent<Image>();
        baseColor = image.color;

        image.raycastTarget = false;
    }

    Color GetColor(float a) {
        baseColor.a = a;
        return baseColor;
    }

    // When transitioning out of a scene
    public void SceneFadeOut(float dur) {
        image.raycastTarget = true;
        StopAllCoroutines();
        StartCoroutine(Fade(0, 1, dur));
    }
    // When transitioning into a scene
    public void SceneFadeIn(float dur) {
        Start();

        image.raycastTarget = false;
        StopAllCoroutines();
        StartCoroutine(Fade(1, 0, dur));
    }

    IEnumerator Fade(int startA, int endA, float dur) {
        image.color = GetColor(startA);
        float time = 0;
        while (time < dur) {
            time += Time.unscaledDeltaTime;
            yield return 0;
            image.color = GetColor(Mathf.Lerp(startA, endA, time / dur));
        }
    }
}
