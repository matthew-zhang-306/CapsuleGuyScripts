using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartMenu : Menu
{
    float quitTimer;

    protected override void Start() {
        base.Start();
        
        if (GameManager.Instance?.ShouldOpenEndMenu ?? false) {
            OpenPanel("End");
            GameManager.Instance.AfterEnd();
        }
        else
            OpenPanel("Main");
    }

    public void NewGame() {
        if (navigationDisabled) return;

        GameManager.Instance?.NewGame();
        AudioManager.Instance?.StopAudio();
        navigationDisabled = true;
    }

    public void EraseSaveData() {
        GameManager.Instance.EraseSavedData();
        GetComponentInChildren<LevelSelectPanel>(true)?.SetReload();
        OpenPanel("Options");
    }

    protected override void Update() {
        base.Update();

        // Quitting logic
        if (Input.GetKey(KeyCode.Escape))
            quitTimer += Time.deltaTime;
        else
            quitTimer = 0;

        if (quitTimer > 1f)
            Application.Quit();
    }
}
