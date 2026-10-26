using UnityEngine;

public class TestDialogue : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.OpenOverlay("DialoguePanel");
        DialogueManager.Instance.StartDialogue("Sword_Dialogue");
    }

}