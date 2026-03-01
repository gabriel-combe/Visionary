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

    public AudioClip VoiceHandOfTheKing => _voiceHandOfTheKing;
    public AudioClip VoiceFool => _voiceFool;
    public AudioClip VoicePrince => _voicePrince;
    public AudioClip VoicePrincess => _voicePrincess;
    public AudioClip VoiceKing => _voiceKing;
    public AudioClip VoiceExecutionner => _voiceExecutionner;
    public AudioClip VoiceServant => _voiceServant;
    public AudioClip VoiceGuard => _voiceGuard;

    public AudioClip MenuMusic => _menuMusic;
    public AudioClip MenuThrone => _menuThrone;
    public AudioClip MenuTorture => _menuTorture;
    public AudioClip MenuGarden => _menuGarden;

    public AudioClip SfxButton => _sfxButton;
    public AudioClip SfxCharacter => _sfxCharacter;
    public AudioClip SfxMenu => _sfxMenu;
    public List<AudioClip> SfxCards => _sfxCards;
    public AudioClip SfxDice => _sfxDice;

    void Awake()
    {
        //subscribe to decor change
        //UIManager.Instance. += changeMusic
    }

    private void changeMusic()
    {
        
    }
}
