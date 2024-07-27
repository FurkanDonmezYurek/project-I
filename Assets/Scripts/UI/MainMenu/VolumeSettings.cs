using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] 
    private Slider _musicSlider, _sfxSlider, _ambianceSlider;

    public void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetAmbianceVolume();
        }

    }
    public void SetMusicVolume()
    {
        AudioManager.instance.MusicVolume(_musicSlider.value);
        float volume = _musicSlider.value;
        myMixer.SetFloat("music", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        AudioManager.instance.SFXVolume(_sfxSlider.value);
        float volume = _sfxSlider.value;
        myMixer.SetFloat("sfx", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetAmbianceVolume()
    {
        AudioManager.instance.AmbianceVolume(_ambianceSlider.value);
        float volume = _ambianceSlider.value;
        myMixer.SetFloat("ambiance", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("ambianceVolume", volume);
    }

    private void LoadVolume()
    {
        _musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        _sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        _ambianceSlider.value = PlayerPrefs.GetFloat("ambianceVolume");

        SetMusicVolume();
        SetSFXVolume();
        SetAmbianceVolume();
    }
}
