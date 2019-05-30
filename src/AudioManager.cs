using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    // Singleton
    protected static AudioManager instance;
    public static AudioManager Instance {
        get { return instance; }
    }

    public Transform cam;
    new FMODUnity.StudioEventEmitter audio;

    FMOD.Studio.Bus busM;
    FMOD.Studio.Bus busS;

    static AudioPaletteList audioList;
    static Dictionary<string, AudioPalette> audioSet;
    bool isUsingTrigger;
    HashSet<string> hitTriggers;

    // Cooldown timer to avoid playing many consecutive crate breaking sounds - this logic really should apply to every sound effect but there's only this one right now
    public float crateTimer;

    void EnforceSingleton() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    protected void Awake() {
        EnforceSingleton();

        busM = FMODUnity.RuntimeManager.GetBus("Bus:/BGM");
        busS = FMODUnity.RuntimeManager.GetBus("Bus:/SFX");
        busM.setVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        busS.setVolume(PlayerPrefs.GetFloat("SoundVolume", 1f));
    }

    protected void OnEnable() {
        if (audioList == null)
            LoadAudio();

        Entity.OnHit += PlayEntityHitSound;
        Entity.OnKilled += PlayEntityKilledSound;
        GroundEntity.OnLand += PlayEntityLandSound;
        PlayerController.OnJump += PlayPlayerJumpSound;
        PlayerController.OnFire += PlayPlayerFireSound;
        //BirdEnemy.OnStartFlap += PlayBirdFlapSound; Removed because without sound effect distance it gets annoying
        ShootingBirdEnemy.OnLock += PlayLaserBirdLockSound;
        ShootingBirdEnemy.OnShoot += PlayLaserBirdFireSound;
        Switch.OnPress += PlaySwitchTriggeredSound;
    }
    protected void OnDisable() {
        Entity.OnHit -= PlayEntityHitSound;
        Entity.OnKilled -= PlayEntityKilledSound;
        GroundEntity.OnLand -= PlayEntityLandSound;
        PlayerController.OnJump -= PlayPlayerJumpSound;
        PlayerController.OnFire -= PlayPlayerFireSound;
        //BirdEnemy.OnStartFlap -= PlayBirdFlapSound;
        ShootingBirdEnemy.OnLock -= PlayLaserBirdLockSound;
        ShootingBirdEnemy.OnShoot -= PlayLaserBirdFireSound;
        Switch.OnPress -= PlaySwitchTriggeredSound;
    }
    
    // Absolutely 100% guarantee that the instance unsubscribes to all of the events
    protected void OnDestroy() => OnDisable();

    private void LoadAudio() {
        audioList = AudioPaletteList.Load("Audios");
        audioSet = new Dictionary<string, AudioPalette>();
        foreach (AudioPalette a in audioList.audios)
            audioSet.Add(a.name, a);
    }

    public void SetMusicVolume(float volume) {
        busM.setVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    public void SetSoundVolume(float volume) {
        busS.setVolume(volume);
        PlayerPrefs.SetFloat("SoundVolume", volume);
    }

    public void DidLoadScene(Scene scene, Room currentRoom) {
        if (audio == null)
            audio = GetComponent<FMODUnity.StudioEventEmitter>();

        if (hitTriggers == null) {
            hitTriggers = new HashSet<string>();
            isUsingTrigger = false;
        }
        if (scene.name.Contains("MENU")) {
            // Reset audio trigger items when returning to menu
            hitTriggers = new HashSet<string>();
            isUsingTrigger = false;
            TransitionToNewPalette("Menu", null);
        }
        
        // Play fallback audio if no audio triggers are active
        if (isUsingTrigger == false && currentRoom != null) {
            TransitionToNewPalette(currentRoom.defaultAudio, null);
        }
        // Play new audio if the room's audio is set to override
        if (currentRoom != null && currentRoom.overrideAudio) {
            TransitionToNewPalette(currentRoom.defaultAudio, null);
            isUsingTrigger = false;
        }
    }

    public void TransitionToNewPalette(string paletteName, string triggerName) {
        // Exit if already playing the desired song
        if (paletteName == null) return;
        
        if (triggerName != null) {
            // Exit if already hit this trigger
            if (hitTriggers.Contains(triggerName)) return;
            
            hitTriggers.Add(triggerName);
            isUsingTrigger = true;
        }

        if (audioSet.ContainsKey(paletteName)) {
            AudioPalette desiredAud = audioSet[paletteName];

            if (!audio.Event.Contains(desiredAud.eventName)) {
                StopAudio();

                audio.Event = "event:/BGM/" + desiredAud.eventName;
                audio.Play();
            }
            
            SetParameter("Intensity", desiredAud.intensity);
            SetParameter("IntensitySlow", desiredAud.intensity);
            
        } else {
            Debug.LogWarning("There is no audio palette named " + paletteName + "!");
        }
    }

    public void StopAudio() {
        if (audio.IsPlaying())
            audio.Stop();
    }

    public void SetParameter(string param, float value) {
        audio.SetParameter(param, value);
    }

    private void Update() {
        crateTimer -= Time.deltaTime;
    }

    void PlaySFX(string path) {
        PlaySFX(path, cam.position);
    }
    void PlaySFX(string path, Vector3 position) {
        FMODUnity.RuntimeManager.PlayOneShot(path, position);
    }


    // ALL SOUND EFFECTS

    void PlayEntityHitSound(Entity e) {
        if (e.GetType() == typeof(PlayerController)) {
            PlaySFX("event:/SFX/Hit");
        }
        else if (e.GetType() == typeof(Crate)) {

        }
        else {
            PlaySFX("event:/SFX/Hit", e.transform.position);
        }
    }
    void PlayEntityKilledSound(Entity e) {
        if (e.GetType() == typeof(PlayerController)) {
            // PlayerDeath
            PlaySFX("event:/SFX/PlayerDeath");
        }
        else if (e.GetType() == typeof(Crate)) {
            // CrateBreak
            if (crateTimer <= 0) {
                PlaySFX("event:/SFX/CrateBreak", e.transform.position);
                crateTimer = 0.5f;
            }
        }
        else {
            PlaySFX("event:/SFX/Hit", e.transform.position);
        }
    }

    void PlayEntityLandSound(Entity e) {
        PlaySFX("event:/SFX/PlayerLand");
    }

    void PlayPlayerJumpSound() {
        PlaySFX("event:/SFX/PlayerJump");
    }
    void PlayPlayerFireSound() {
        PlaySFX("event:/SFX/LaserShoot");
    }

    void PlayBirdFlapSound(BirdEnemy bird) {
        PlaySFX("event:/SFX/BirdFlap", bird.transform.position);
    }
    void PlayLaserBirdLockSound(BirdEnemy bird) {
        PlaySFX("event:/SFX/LaserBirdLock");
    }
    void PlayLaserBirdFireSound(BirdEnemy bird) {
        PlayPlayerFireSound();
    }

    void PlaySwitchTriggeredSound(Switch s) {
        if (s.GetType() == typeof(Button))
            PlaySFX("event:/SFX/ButtonPress", s.transform.position);
    }
}
