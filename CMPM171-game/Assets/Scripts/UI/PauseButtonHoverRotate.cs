using UnityEngine;
using UnityEngine.EventSystems;

public class PauseButtonHoverRotate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform target;

    public float hoverAngle = -90f;
    public float duration = 0.22f;
    public bool useUnscaledTime = true;

    public CanvasGroup hoverOverlay;
    public float overlayAlpha = 0.15f;
    public float overlayFadeDuration = 0.12f;

    private Coroutine rotateCo;
    private Coroutine overlayCo;
    private Quaternion baseRot;

    void Awake()
    {
        if (target == null) target = transform as RectTransform;
        baseRot = target.localRotation;

        if (hoverOverlay != null)
            hoverOverlay.alpha = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FadeOverlayTo(overlayAlpha);
        RotateTo(Quaternion.Euler(0, 0, hoverAngle));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FadeOverlayTo(0f);
        RotateTo(baseRot);
    }

    private void RotateTo(Quaternion toRot)
    {
        if (rotateCo != null) StopCoroutine(rotateCo);
        rotateCo = StartCoroutine(RotateAnim(toRot));
    }

    private System.Collections.IEnumerator RotateAnim(Quaternion toRot)
    {
        Quaternion fromRot = target.localRotation;
        float t = 0f;

        while (t < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;

            float p = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);

            target.localRotation = Quaternion.Slerp(fromRot, toRot, eased);
            yield return null;
        }

        target.localRotation = toRot;
    }

    private void FadeOverlayTo(float to)
    {
        if (hoverOverlay == null) return;
        if (overlayCo != null) StopCoroutine(overlayCo);
        overlayCo = StartCoroutine(OverlayAnim(to));
    }

    private System.Collections.IEnumerator OverlayAnim(float to)
    {
        float from = hoverOverlay.alpha;
        float t = 0f;

        while (t < overlayFadeDuration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;

            float p = Mathf.Clamp01(t / overlayFadeDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);

            hoverOverlay.alpha = Mathf.Lerp(from, to, eased);
            yield return null;
        }

        hoverOverlay.alpha = to;
    }
}