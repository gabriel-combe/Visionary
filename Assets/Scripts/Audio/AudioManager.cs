using System.Collections;
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
    [SerializeField] private AudioClip _sfxOssicle;

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
    public AudioClip SfxOssicle => _sfxOssicle;

    public float fadeDuration = 1.0f;

    void OnEnable()
    {
        EventBus.Subscribe<EnvironmentChangedEvent>(OnEnvironmentChanged);
        EventBus.Subscribe<OssicleViewChangedEvent>(OnOssicleViewChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<EnvironmentChangedEvent>(OnEnvironmentChanged);
        EventBus.Unsubscribe<OssicleViewChangedEvent>(OnOssicleViewChanged);
    }

    private void OnEnvironmentChanged(EnvironmentChangedEvent e) => ChangeMusicEnv(e.Index);
    private void OnOssicleViewChanged(OssicleViewChangedEvent e) => ChangeMusicOss(e.IsOssicleView);

    public void ChangeMusicOss(bool ossicleView)
    {
        StopAllCoroutines();
        StartCoroutine(FadeInOut(ossicleView ? _menuMusic : _menuThrone));
    }

    public void ChangeMusicEnv(int id)
    {
        StopAllCoroutines();
        switch (id)
        {
            case 0:
                StartCoroutine(FadeInOut(_menuThrone));
                break;
            case 1:
            case 3:
                StartCoroutine(FadeInOut(_menuGarden));
                break;
            case 2:
                StartCoroutine(FadeInOut(_menuTorture));
                break;
            default: break;
        }
    }

    private IEnumerator FadeInOut(AudioClip clip)
    {
        if(_musicSource.clip != clip)
        {
            float startVolume = _musicSource.volume;
            float timer = 0;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
                yield return null;
            }

            // Ensure it's exactly 0 and stop
            _musicSource.volume = 0;
            _musicSource.Stop();

            // --- 2. SWITCH CLIP ---
            _musicSource.clip = clip;
            _musicSource.Play();

            // --- 3. FADE IN ---
            timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0, 1, timer / fadeDuration);
                yield return null;
            }

            _musicSource.volume = 1;
        }
    }
}
