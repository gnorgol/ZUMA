using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using System;

public class SfxManager : MonoBehaviour
{
    [SerializeField] private AudioClip PlayerShootSound,ClickButton,MusicMenu,MusicGameplay;
    private AudioSource audioSrc;
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
    }
    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlayerShootEvent>(OnPlayerShoot);
        EventManager.Instance.RemoveListener<GameMenuEvent>(OnGameMenu);
        EventManager.Instance.RemoveListener<GamePlayEvent>(OnGamePlay);

    }

    private void OnPlayerShoot(PlayerShootEvent e)
    {
        audioSrc.PlayOneShot(PlayerShootSound);
    }
    private void OnGameMenu(GameMenuEvent e)
    {
        audioSrc.clip = MusicMenu;
        audioSrc.Play();
    }
    private void OnGamePlay(GamePlayEvent e)
    {
        audioSrc.clip = MusicGameplay;
        audioSrc.Play();
    }


    private void Start()
    {
        
        audioSrc = GetComponent<AudioSource>();
    }


    public void PlayClickButton()
    {
        audioSrc.PlayOneShot(ClickButton);
    }

}
