using System.Collections.Generic;
using UnityEngine;

//public class AudioManager : SingletonMono<AudioManager>
public class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] public AudioSource _musicSource;
    [SerializeField] public AudioSource _sfxSource;

    [Header("Music")]
    [SerializeField] public AudioClip _menuMusic;
    [SerializeField] public AudioClip _menuThrone;
    [SerializeField] public AudioClip _menuTorture;
    [SerializeField] public AudioClip _menuGarden;

    [Header("SFX")]
    [SerializeField] public AudioClip _sfxButton;
    [SerializeField] public AudioClip _sfxCharacter;
    [SerializeField] public AudioClip _sfxMenu;
    [SerializeField] public List<AudioClip> _sfxCards;
    [SerializeField] public AudioClip _sfxDice;

    [Header("Voices")]
    [SerializeField] public AudioClip _voiceHandOfTheKing;
    [SerializeField] public AudioClip _voiceFool;
    [SerializeField] public AudioClip _voicePrince;
    [SerializeField] public AudioClip _voicePrincess;
    [SerializeField] public AudioClip _voiceKing;
    [SerializeField] public AudioClip _voiceExecutionner;
    [SerializeField] public AudioClip _voiceServant;
    [SerializeField] public AudioClip _voiceGuard;
    
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

    public void changeMusic()
    {
        
    }
}
