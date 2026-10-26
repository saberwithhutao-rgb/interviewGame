using UnityEngine;

public class PlayerHUDController : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    private void OnEnable()
    {
        DialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= OnDialogueStarted;
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        }
    }

    private void OnDialogueStarted()
    {
        SetCanvasVisible(false);
    }

    private void OnDialogueEnded()
    {
        SetCanvasVisible(true);
    }

    public void SetCanvasVisible(bool visible)
    {
        _canvas.enabled = visible;
    }

}