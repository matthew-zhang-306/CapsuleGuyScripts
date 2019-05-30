using UnityEngine;

public class PauseMenu : Menu
{
    bool paused;

    float pInput;
    float oldPInput;

    // :)
    public GameObject smileyBall;

    protected override void Start() {
        base.Start();
        
        OpenPanel("close");
    }

    protected override void Update() {
        base.Update();

        // The pause menu should not be openable during important transitions because it will break very very hard
        if (GameManager.Instance?.IsInTransition ?? false)
            navigationDisabled = true;

        oldPInput = pInput;
        pInput = Input.GetAxisRaw("Pause");

        if (pInput > 0 && oldPInput == 0)
            Pause();
    }

    public void Pause() {
        if (navigationDisabled) return;
        
        paused = !paused;
        OpenPanel(paused ? "pause" : "close");
        Time.timeScale = paused ? 0 : 1; // oh gosh time scale
    }

    public void ResetLevel() {
        if (navigationDisabled) return;

        Pause();
        GameManager.Instance.ResetLevel();
        navigationDisabled = true;
    }

    public void BackToMenu() {
        if (navigationDisabled) return;

        GameManager.Instance?.BackToMenu();
        AudioManager.Instance?.StopAudio();
        navigationDisabled = true;
    }


    // Lol
    public void SpawnSmileyBall() {
        GameObject.Instantiate(smileyBall, transform.parent);
    }
}
