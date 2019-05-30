using System.Collections.Generic;
using UnityEngine;

public class AlarmController : Controllable
{
    bool alarmOn;
    public BackgroundLayer[] bgs;
    
    List<SpriteRenderer> backgrounds;
    List<Color> baseColors;

    float pulseTimer;
    public float pulseTime;
    public float intensity;

    public override void Switch() {
        alarmOn = true;
    }

    private void Start() {
        backgrounds = new List<SpriteRenderer>();
        baseColors = new List<Color>();
        
        // Create parallel lists of all background sprites and their colors
        foreach (BackgroundLayer bg in bgs) {
            backgrounds.AddRange(bg.GetAllSprites());
        }
        foreach (SpriteRenderer background in backgrounds) {
            baseColors.Add(background.color);
        }
    }

    private void Update() {
        if (!alarmOn) return;

        pulseTimer = (pulseTimer + Time.deltaTime) % pulseTime;
        for (int s = 0; s < backgrounds.Count; s++) {
            SpriteRenderer sr = backgrounds[s];
            Color baseColor = baseColors[s];

            // Pulse extra red during the first half of the time
            float red = Mathf.Lerp(baseColor.r * intensity, baseColor.r, pulseTimer * 2 / pulseTime);
            sr.color = new Color(red, baseColor.g, baseColor.b, baseColor.a);
        }
    }
}
