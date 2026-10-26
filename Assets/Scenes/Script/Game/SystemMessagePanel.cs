// SystemMessagePanel.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SystemMessagePanel : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("动画参数")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("颜色配置")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color achievementColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color combatColor = new Color(1f, 0.5f, 0f);

    private RectTransform rectTransform;
    private Vector2 hiddenPosition;
    private Vector2 showPosition;
    private Coroutine currentAnimation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private void Start()
    {
        float panelHeight = rectTransform.rect.height;
        hiddenPosition = new Vector2(0f, panelHeight + 20f);
        showPosition = new Vector2(0f, -50f);
        rectTransform.anchoredPosition = hiddenPosition;
    }

    public void SetText(string content)
    {
        messageText.text = content;
    }

    public void SetMessageType(MessageType type)
    {
        Color color = GetColorByType(type);
        //messageText.color = color;
        messageText.outlineWidth = 0.2f;
        messageText.outlineColor = Color.black;
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }

    public void Show(System.Action onComplete = null)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(ShowAnimation(onComplete));
    }

    public void Hide(System.Action onComplete = null)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(HideAnimation(onComplete));
    }

    private IEnumerator ShowAnimation(System.Action onComplete)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = hiddenPosition;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            float t = elapsed / slideDuration;
            t = t * t * (3f - 2f * t); // SmoothStep
            rectTransform.anchoredPosition = Vector2.Lerp(hiddenPosition, showPosition, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = showPosition;
        canvasGroup.alpha = 1f;
        currentAnimation = null;
        onComplete?.Invoke();
    }

    private IEnumerator HideAnimation(System.Action onComplete)
    {
        float elapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            t = t * t; // EaseIn
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, hiddenPosition, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        currentAnimation = null;
        onComplete?.Invoke();
    }

    private Color GetColorByType(MessageType type)
    {
        switch (type)
        {
            case MessageType.Success: return successColor;
            case MessageType.Warning: return warningColor;
            case MessageType.Error: return errorColor;
            case MessageType.Achievement: return achievementColor;
            case MessageType.Combat: return combatColor;
            default: return normalColor;
        }
    }
}