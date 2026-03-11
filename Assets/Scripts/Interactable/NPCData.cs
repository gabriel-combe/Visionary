using UnityEngine;

public class NPCData : MonoBehaviour
{
    [SerializeField] private ConversationGraph _dialogGraph;

    public ConversationGraph DialogGraph => _dialogGraph;
}
