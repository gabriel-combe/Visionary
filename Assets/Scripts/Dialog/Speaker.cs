using UnityEngine;

/// <summary>
/// Identifies a character that can speak in a ConversationGraph.
/// One SO per character - drag and drop onto DialogNodes.
/// </summary>
[CreateAssetMenu(menuName = "Dialog/Speaker", fileName = "Speaker")]
public class Speaker : ScriptableObject
{
    [SerializeField] private string _npcId;
    [SerializeField] private Sprite _portrait;
    [SerializeField] private AudioClip _voiceClip;

    public string NpcId => _npcId;
    public Sprite Portrait => _portrait;
    public AudioClip VoiceClip => _voiceClip;
}
