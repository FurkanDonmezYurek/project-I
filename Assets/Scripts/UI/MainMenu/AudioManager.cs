using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager instance; //Access to everywhere

    public Sound[] musicSounds, sfxSounds, ambianceSounds;
    public AudioSource musicSource, sfxSource, ambianceSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == 2)
        {

            PlayMusic("MainThemeMusic"); //Play Backround
        }else if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            PlayMusic("LobbyThemeMusic");
        }

    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x=> x.name== name);

        if (s == null) //if there is no sound
        {
            Debug.Log("Sound Not Found");
        }
        else { 
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name) {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Sound Not Found");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip); //check the document.
        }
    }

    public void PlayAmbiance(string name)
    {
        Sound s = Array.Find(ambianceSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Sound Not Found");
        }
        else
        {
            ambianceSource.PlayOneShot(s.clip); //check the document.
        }
    }

    public void MusicVolume(float volume) //Change volume
    {
        musicSource.volume = volume;
    }
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
    public void AmbianceVolume(float volume)
    {
        ambianceSource.volume = volume;
    }

    /*
    public void ToggleMusic() //Shut up or Play Music
    {
        musicSource.mute = !musicSource.mute;
    }
    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    public void ToggleAmbiance()
    {
        ambianceSource.mute = !ambianceSource.mute;
    }*/

}
