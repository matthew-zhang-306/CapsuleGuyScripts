using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MenuPanel
{
    Slider sliderM;
    Slider sliderS;

    protected override void Start() {
        Slider[] sliders = GetComponentsInChildren<Slider>();
        sliderM = sliders[0];
        sliderS = sliders[1];

        sliderM.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sliderS.value = PlayerPrefs.GetFloat("SoundVolume", 1f);

        base.Start();
    }

    public void SetMusicVolume(float volume) {
        AudioManager.Instance.SetMusicVolume(volume);
    }
    public void SetSoundVolume(float volume) {
        AudioManager.Instance.SetSoundVolume(volume);
    }

    // Note: erase save data logic located in GameManager instead
}
