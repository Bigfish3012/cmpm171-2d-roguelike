using UnityEngine;
using UnityEngine.EventSystems;

public class PauseButtonHoverRotate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform target;                                                         // Target transform to rotate

    public float hoverAngle = -90f;                                                      // Rotation angle on hover
    public float duration = 0.22f;                                                       // Rotation animation duration
    public bool useUnscaledTime = true;                                                  // Whether to use unscaled time

    public CanvasGroup hoverOverlay;                                                     // Overlay shown on hover
    public float overlayAlpha = 0.15f;                                                   // Target alpha for overlay
    public float overlayFadeDuration = 0.12f;                                            // Fade animation duration

    private Coroutine rotateCo;                                                          // Active rotation coroutine
    private Coroutine overlayCo;                                                         // Active overlay fade coroutine
    private Quaternion baseRot;                                                          // Original rotation

    // Cache the base rotation and hide overlay
    void Awake()
    {
        if (target == null) target = transform as RectTransform;
        baseRot = target.localRotation;

        if (hoverOverlay != null)
            hoverOverlay.alpha = 0f;
    }

    // Start rotation and overlay fade on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        FadeOverlayTo(overlayAlpha);
        RotateTo(Quaternion.Euler(0, 0, hoverAngle));
    }

    // Reset rotation and overlay on pointer exit
    public void OnPointerExit(PointerEventData eventData)
    {
        FadeOverlayTo(0f);
        RotateTo(baseRot);
    }

    // Stop all coroutines and reset visuals when disabled
    void OnDisable()
    {
        if (rotateCo != null) StopCoroutine(rotateCo);
        if (overlayCo != null) StopCoroutine(overlayCo);

        if (target != null)
            target.localRotation = baseRot;
        if (hoverOverlay != null)
            hoverOverlay.alpha = 0f;
    }

    // Start a rotation animation to the target rotation
    private void RotateTo(Quaternion toRot)
    {
        if (rotateCo != null) StopCoroutine(rotateCo);
        rotateCo = StartCoroutine(RotateAnim(toRot));
    }

    // Coroutine that animates rotation with ease-out
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

    // Start a fade animation on the overlay
    private void FadeOverlayTo(float to)
    {
        if (hoverOverlay == null) return;
        if (overlayCo != null) StopCoroutine(overlayCo);
        overlayCo = StartCoroutine(OverlayAnim(to));
    }

    // Coroutine that animates overlay alpha with ease-out
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
