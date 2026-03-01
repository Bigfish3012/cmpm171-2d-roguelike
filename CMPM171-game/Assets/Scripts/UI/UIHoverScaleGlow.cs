using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverScaleGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform target;
    public CanvasGroup glow;

    public float normalScale = 1f;
    public float hoverScale = 1.08f;

    public float normalGlow = 0f;
    public float hoverGlow = 0.7f;

    public float duration = 0.12f;
    public bool useUnscaledTime = true;

    Coroutine co;

    void Awake()
    {
        if (target == null) target = transform as RectTransform;
        if (glow != null) glow.alpha = normalGlow;
        target.localScale = Vector3.one * normalScale;
    }

    public void OnPointerEnter(PointerEventData eventData) => Play(true);
    public void OnPointerExit(PointerEventData eventData) => Play(false);

    void Play(bool hover)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Anim(hover));
    }

    System.Collections.IEnumerator Anim(bool hover)
    {
        float fromS = target.localScale.x;
        float toS = hover ? hoverScale : normalScale;

        float fromA = glow ? glow.alpha : 0f;
        float toA = hover ? hoverGlow : normalGlow;

        float t = 0f;
        while (t < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;

            float p = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);

            target.localScale = Vector3.one * Mathf.Lerp(fromS, toS, eased);
            if (glow) glow.alpha = Mathf.Lerp(fromA, toA, eased);

            yield return null;
        }

        target.localScale = Vector3.one * toS;
        if (glow) glow.alpha = toA;
    }
}