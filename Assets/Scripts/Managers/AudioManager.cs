using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMono<AudioManager>
{
    [Header("Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _menuThrone;
    [SerializeField] private AudioClip _menuTorture;
    [SerializeField] private AudioClip _menuGarden;

    [Header("SFX")]
    [SerializeField] private AudioClip _sfxButton;
    [SerializeField] private AudioClip _sfxCharacter;
    [SerializeField] private AudioClip _sfxMenu;
    [SerializeField] private List<AudioClip> _sfxCards;
    [SerializeField] private AudioClip _sfxDice;

    [Header("Voices")]
    [SerializeField] private AudioClip _voiceHandOfTheKing;
    [SerializeField] private AudioClip _voiceFool;
    [SerializeField] private AudioClip _voicePrince;
    [SerializeField] private AudioClip _voicePrincess;
    [SerializeField] private AudioClip _voiceKing;
    [SerializeField] private AudioClip _voiceExecutionner;
    [SerializeField] private AudioClip _voiceServant;
    [SerializeField] private AudioClip _voiceGuard;
    
    public AudioSource MusicSource => _musicSource;
    public AudioSource SfxSource => _sfxSource;
}
