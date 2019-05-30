using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /* SINGLETON */
    protected static GameManager instance;
    public static GameManager Instance {
        get { return instance; }
    }
    protected void EnforceSingleton() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    protected void Awake() {
        EnforceSingleton();
    }

    /* ROOM MANAGEMENT */
    static RoomList roomList;
    static List<Room> roomsForLevelSelect;
    public static List<Room> Rooms { get { return roomsForLevelSelect; }}
    static Dictionary<string, Room> roomSet;
    Room currentRoom;
    Room previousRoom;

    /* MENU */
    public string menuSceneStr;
    Scene menuScene;

    /* ESSENTIAL LEVEL OBJECTS */
    PlayerController player;
    public PlayerController Player { get { return player; }}
    CameraFollow cam;
    public GameObject Cam { get { return cam?.gameObject; }}
    Canvas canvas;
    EventSystem eventSystem;

    /* TRANSITIONS */
    bool didFadeOut;
    public float fadeTime;
    public bool IsInTransition;

    /* LEVEL PROGRESSION TRACKING */
    bool playerHasLaser;
    public bool PlayerHasLaser { get { return playerHasLaser; }}
    float deathCount;
    public float DeathCount { get { return deathCount; }}
    bool shouldOpenEndMenu;
    public bool ShouldOpenEndMenu { get { return shouldOpenEndMenu; }}

    private void OnEnable() {
        if (roomList == null)
            LoadRooms();

        SceneManager.sceneLoaded += DidLoadScene;
        PlayerController.OnPlayerDeath += OnPlayerDeath;
    }
    private void OnDisable() {
        SceneManager.sceneLoaded -= DidLoadScene;
        PlayerController.OnPlayerDeath -= OnPlayerDeath;
    }
    private void OnDestroy() => OnDisable();

    private void LoadRooms() {
        roomList = RoomList.Load("Rooms");
        roomsForLevelSelect = new List<Room>();
        roomSet = new Dictionary<string, Room>();

        // First, put all the rooms in the dictionary
        foreach (Room r in roomList.rooms) {
            roomSet.Add(r.name, r);
            if (!r.ignoreInLevelSelect)
                roomsForLevelSelect.Add(r);
        }
        
        // Then, load in content
        Room prevRoom = null;
        foreach (Room r in roomList.rooms) {
            // Set two-way level flow
            if (r.nextRoom != null && roomSet.ContainsKey(r.nextRoom))
                roomSet[r.nextRoom].previousRoom = r.name;
            if (r.secretRoom != null && roomSet.ContainsKey(r.secretRoom))
                roomSet[r.secretRoom].previousRoom = r.name;

            // Fill in level groups and music fallbacks
            if (prevRoom != null) {
                if (r.defaultAudio == null)
                    r.defaultAudio = prevRoom.defaultAudio;
                if (r.group == null)
                    r.group = prevRoom.group;
            }

            prevRoom = r;
        }
    }

    /* LEVEL LOADING METHODS */
    void DidLoadScene(Scene scene, LoadSceneMode mode) {
        Time.timeScale = 1;
        IsInTransition = false;
        GetEverything(scene.name);
        eventSystem = EventSystemSingleton.Instance;

        if (didFadeOut) {
            FadeTransition fade = canvas.GetComponentInChildren<FadeTransition>();
            if (fade != null) {
                didFadeOut = false;
                fade.SceneFadeIn(fadeTime / 2);
            }
        }

        AudioManager aud = GetComponent<AudioManager>();

        if (!scene.name.Contains("MENU")) {
            if (roomSet.ContainsKey(scene.name))
                currentRoom = roomSet[scene.name];
            
            PlacePlayer();
            if (!playerHasLaser && currentRoom != null && currentRoom.noLaser)
                player.GetComponentInChildren<LaserGun>(true)?.gameObject.SetActive(false);
            else {
                playerHasLaser = true;
                player.GetComponentInChildren<LaserGun>(true)?.gameObject.SetActive(true);
            }

            aud.cam = cam.transform;

            PlayerPrefs.SetInt(scene.name, 1);
        }
        else {
            // Reset level progression things upon entering menu
            currentRoom = null;
            playerHasLaser = false;
            deathCount = 0;
        }

        aud.DidLoadScene(scene, currentRoom);
    }

    void GetEverything(string sceneName) {
        if (!sceneName.Contains("MENU")) {
            player = FindScript<PlayerController>("Player", "PlayerController");
            cam = FindScript<CameraFollow>("MainCamera", "CameraFollow");
        }
        else {
            player = null;
            cam = null;
        }

        canvas = FindScript<Canvas>("Canvas", "Canvas");
    }

    T FindScript<T>(string tagName, string scriptName) {
        GameObject g = GameObject.FindGameObjectWithTag(tagName);
        if (g == null) {
            Debug.LogError("Scene " + SceneManager.GetActiveScene().name + " does not contain an object with tag " + tagName + "!");
            return default(T);
        }

        T c = g.GetComponent<T>();
        if (c == null) {
            Debug.LogError("GameObject " + g.name + " does not contain a script " + scriptName + "!");
            return default(T);
        }

        return c;
    }

    void PlacePlayer() {
        if (previousRoom == null) return;

        foreach(GameObject lz in GameObject.FindGameObjectsWithTag("LoadingZone")) {
            LoadingZone load = lz.GetComponent<LoadingZone>();
            if (load != null && load.GetTarget(currentRoom) == previousRoom.name) {
                player.transform.position = load.SpawnPoint;
                break;
            }
        }
    }

    /* SCENE LOADING + TRANSITION BASE METHODS */
    void SceneFadeOut(float time) {
        IsInTransition = true;

        FadeTransition fade = canvas.GetComponentInChildren<FadeTransition>();
        if (fade != null) {
            didFadeOut = true;
            fade.SceneFadeOut(time);
        }
    }
    IEnumerator LoadScene(string name, float delay) {
        IsInTransition = true;
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(name);
    }

    /* SCENE MANAGEMENT UTILITY METHODS */
    public void LoadLevel(string name) {
        SceneFadeOut(fadeTime);
        StartCoroutine(LoadScene(name, fadeTime));
    }
    public void StartSceneTransition(LoadingZone load) {
        previousRoom = currentRoom;
        player.StartLevelTransition(load.exitDirection);
        LoadLevel(load.GetTarget(currentRoom));
    }
    public void NewGame() {
        LoadLevel(roomList.FirstRoom.name);
    }
    public void BackToMenu() {
        LoadLevel(menuSceneStr);
    }

    public void ResetLevel() {
        if (player != null)
            player.ForceDeath();
        else
            OnPlayerDeath();
    }
    void OnPlayerDeath() {
        StartCoroutine(ReloadScene(fadeTime / 2, fadeTime / 2));
        deathCount++;
    }

    IEnumerator ReloadScene(float delay, float time) {
        yield return new WaitForSeconds(delay);
        LoadLevel(SceneManager.GetActiveScene().name);
    }
    
    /* MISCELLANEOUS */
    public void PlayerGotLaser() {
        playerHasLaser = true;
        player.ActivateLaserGun();
    }

    public void HitEnd() {
        IsInTransition = true;
        shouldOpenEndMenu = true;
        PlayerPrefs.SetInt("beatGame", 1);
    }
    public void AfterEnd() {
        shouldOpenEndMenu = false;
    }

    public void EraseSavedData() {
        foreach (Room r in roomList.rooms)
            PlayerPrefs.SetInt(r.name, 0);
        PlayerPrefs.SetInt("beatGame", 0);
    }
}
