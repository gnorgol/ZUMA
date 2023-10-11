using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using System;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Runtime.CompilerServices;

public class SfxManager : MonoBehaviour
{
    [SerializeField] private AudioClip PlayerShootSound,ClickButton,MusicMenu,MusicGameplay,GainScore;
    private AudioSource audioSrc;
    private AudioMixer audioMixer;
    [SerializeField] AudioSource audioMusic;
    [SerializeField] AudioSource audioSFX;

    [SerializeField] Slider sliderMaster;
    [SerializeField] Slider sliderMusic;
    [SerializeField] Slider sliderSFX;

    [SerializeField] TextMeshProUGUI textPourcentageMaster;
    [SerializeField] TextMeshProUGUI textPourcentageMusic;
    [SerializeField] TextMeshProUGUI textPourcentageSFX;

    private bool isInitilized = false;

    private void OnEnable()
    {
        SubscribeEvents();
    }
    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    private void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlayerShootEvent>(OnPlayerShoot);
        EventManager.Instance.AddListener<GameMenuEvent>(OnGameMenu);
        EventManager.Instance.AddListener<GamePlayEvent>(OnGamePlay);
        EventManager.Instance.AddListener<GainScoreEvent>(OnGainScore);
        EventManager.Instance.AddListener<GameSettingEvent>(OnGameSetting);
    }


    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlayerShootEvent>(OnPlayerShoot);
        EventManager.Instance.RemoveListener<GameMenuEvent>(OnGameMenu);
        EventManager.Instance.RemoveListener<GamePlayEvent>(OnGamePlay);
        EventManager.Instance.RemoveListener<GainScoreEvent>(OnGainScore);
        EventManager.Instance.RemoveListener<GameSettingEvent>(OnGameSetting);

    }

    private void OnGainScore(GainScoreEvent e)
    {
        audioSFX.PlayOneShot(GainScore);
    }

    private void OnPlayerShoot(PlayerShootEvent e)
    {
        audioSFX.PlayOneShot(PlayerShootSound);
    }
    private void OnGameMenu(GameMenuEvent e)
    {
        //if MusicMenu is not play


        if (audioMusic.clip != MusicMenu)
        {
           /* audioSrc.clip = MusicMenu;
            audioSrc.Play();*/
           audioMusic.clip = MusicMenu;
           audioMusic.Play();
        }
    }
    private void OnGamePlay(GamePlayEvent e)
    {
        /*audioSrc.clip = MusicGameplay;
        audioSrc.Play();*/
        audioMusic.clip = MusicGameplay;
        audioMusic.Play();
    }
    private void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        audioMixer = audioSrc.outputAudioMixerGroup.audioMixer;
    }


    public void PlayClickButton()
    {
        audioSFX.PlayOneShot(ClickButton);
    }
    private void OnGameSetting(GameSettingEvent e)
    {
        SetSlider();
        SetPourcentage();
        isInitilized = true;
    }
    private void SetSlider()
    {
        float value;
        audioMixer.GetFloat("MasterVolume", out value);
        //convert db to linear
        value = Mathf.Pow(10, value / 20);
        sliderMaster.value = value;
        audioMixer.GetFloat("MusicVolume", out value);
        value = Mathf.Pow(10, value / 20);
        sliderMusic.value = value;
        audioMixer.GetFloat("SFXVolume", out value);
        
        value = Mathf.Pow(10, value / 20);
        sliderSFX.value = value;
    }
    private void SetPourcentage()
    {
        float value;
        audioMixer.GetFloat("MasterVolume", out value);
        //convert db to linear
        value = Mathf.Pow(10, value / 20);
        textPourcentageMaster.text = Mathf.RoundToInt(value * 100) + "%";
        audioMixer.GetFloat("MusicVolume", out value);
        value = Mathf.Pow(10, value / 20);
        textPourcentageMusic.text = Mathf.RoundToInt(value * 100) + "%";
        audioMixer.GetFloat("SFXVolume", out value);
        value = Mathf.Pow(10, value / 20);
        textPourcentageSFX.text = Mathf.RoundToInt(value * 100) + "%";
    }
    private void SetPourcentage(TextMeshProUGUI text, float value)
    {
        text.text = Mathf.RoundToInt(value * 100) + "%";
    }
    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        //set Pourcentage
        SetPourcentage(textPourcentageMaster, value);
    }
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        SetPourcentage(textPourcentageMusic, value);
    }
    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        SetPourcentage(textPourcentageSFX, value);
        //play sfx to test volume
        if (!audioSFX.isPlaying && isInitilized)
        {
            audioSFX.PlayOneShot(GainScore);
        }
        
    }

}
