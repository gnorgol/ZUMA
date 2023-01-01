using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using System;
using UnityEngine.UI;

public class SfxManager : MonoBehaviour
{
    [SerializeField] private AudioClip PlayerShootSound,ClickButton,MusicMenu,MusicGameplay,GainScore;
    private AudioSource audioSrc;
    [SerializeField] private Sprite MusicOn, MusicOff;
    [SerializeField] private List<Button> ListMuteButton;

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
        EventManager.Instance.AddListener<ClickButtonMuteEvent>(OnClickButtonMute);
    }
    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlayerShootEvent>(OnPlayerShoot);
        EventManager.Instance.RemoveListener<GameMenuEvent>(OnGameMenu);
        EventManager.Instance.RemoveListener<GamePlayEvent>(OnGamePlay);
        EventManager.Instance.RemoveListener<GainScoreEvent>(OnGainScore);
        EventManager.Instance.RemoveListener<ClickButtonMuteEvent>(OnClickButtonMute);


    }

    private void OnGainScore(GainScoreEvent e)
    {
        audioSrc.PlayOneShot(GainScore);
    }

    private void OnPlayerShoot(PlayerShootEvent e)
    {
        audioSrc.PlayOneShot(PlayerShootSound);
    }
    private void OnGameMenu(GameMenuEvent e)
    {
        //if MusicMenu is not play


        if (audioSrc.clip != MusicMenu)
        {
            audioSrc.clip = MusicMenu;
            audioSrc.Play();
        }
    }
    private void OnGamePlay(GamePlayEvent e)
    {
        audioSrc.clip = MusicGameplay;
        audioSrc.Play();
    }
    public bool IsMute
    {
        get { return PlayerPrefs.GetInt("IsMute", 0) == 1; }
        set
        {
            PlayerPrefs.SetInt("IsMute", value ? 1 : 0);
            PlayerPrefs.Save();
            audioSrc.mute = value;
        }
    }
    private void Awake()
    {        
        audioSrc = GetComponent<AudioSource>();
        audioSrc.mute = IsMute;
        foreach (Button button in ListMuteButton)
        {
            //set the sprite of the button
            button.image.sprite = IsMute ? MusicOff : MusicOn;
        }
    }


    public void PlayClickButton()
    {
        audioSrc.PlayOneShot(ClickButton);
    }
    private void OnClickButtonMute(ClickButtonMuteEvent e)
    {
        IsMute = !IsMute;
        e.button.GetComponent<UnityEngine.UI.Image>().sprite = IsMute ? MusicOff : MusicOn;
    }

}
