using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip menuThrone;
    [SerializeField] private AudioClip menuTorture;
    [SerializeField] private AudioClip menuGarden;

    [Header("SFX")]
    [SerializeField] private AudioClip sfxButton;
    [SerializeField] private AudioClip sfxCharacter;
    [SerializeField] private AudioClip sfxMenu;
    [SerializeField] private List<AudioClip> sfxCards;
    [SerializeField] private AudioClip sfxDice;

    [Header("Voices")]
    [SerializeField] private AudioClip voiceHandOfTheKing;
    [SerializeField] private AudioClip voiceFool;
    [SerializeField] private AudioClip voicePrince;
    [SerializeField] private AudioClip voicePrincess;
    [SerializeField] private AudioClip voiceKing;
    [SerializeField] private AudioClip voiceExecutionner;
    [SerializeField] private AudioClip voiceServant;
    [SerializeField] private AudioClip voiceGuard;
}
