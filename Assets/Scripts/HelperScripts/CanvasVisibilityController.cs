using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasVisibilityController : MonoBehaviour
{
    public bool useUnscaledTime;
    public bool startHidden;
    [SerializeField][Range(1, 15)] float smoothing;
    [Header("Debug")]
    [SerializeField] CanvasGroup canvas;
    [SerializeField] float targetAlpha;

    private void Awake()
    {
        canvas = GetComponentInChildren<CanvasGroup>();

        if (startHidden)
        {
            targetAlpha = 0;
            canvas.alpha = 0;
            canvas.blocksRaycasts = false;
        }

        else
        {
            targetAlpha = 1;
            canvas.alpha = 1;
            canvas.blocksRaycasts = true;
        }
    }

    private void LateUpdate()
    {


        float currentDeltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float currentAlpha = canvas.alpha;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, currentDeltaTime * smoothing);

        float diff = Mathf.Abs(currentAlpha - targetAlpha);
        if (diff < 0.01f) currentAlpha = targetAlpha;

        canvas.alpha = currentAlpha;
        canvas.blocksRaycasts = currentAlpha > 0.99f;
    }

    public void Show()
    {
        targetAlpha = 1;
    }

    public void Hide()
    {
        targetAlpha = 0;
    }
}