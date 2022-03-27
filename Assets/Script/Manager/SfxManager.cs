using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using System;

public class SfxManager : MonoBehaviour
{
    [SerializeField] private AudioClip PlayerShootSound,ClickButton;
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
    }
    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlayerShootEvent>(OnPlayerShoot);

    }

    private void OnPlayerShoot(PlayerShootEvent e)
    {
        audioSrc.PlayOneShot(PlayerShootSound);
    }
    public void PlayClickButton()
    {
        audioSrc.PlayOneShot(ClickButton);
    }
    private void Start()
    {
        
        audioSrc = GetComponent<AudioSource>();
    }
    
    
    
    
}
