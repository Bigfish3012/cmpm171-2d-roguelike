using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuItemHoverSmooth : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Refs")]
    public CanvasGroup hoverBG;                                                          // Background overlay shown on hover
    public CanvasGroup activeBar;                                                        // Side bar shown on hover

    [Header("Tuning")]
    public float hoverBGAlpha = 0.15f;                                                   // Target alpha for hover background
    public float moveX = 14f;                                                            // Horizontal offset when hovered
    public float scale = 1.03f;                                                          // Scale multiplier when hovered
    public float speed = 12f;                                                            // Interpolation speed

    Vector3 originalPos;                                                                 // Original local position
    Vector3 targetPos;                                                                   // Target local position

    Vector3 originalScale;                                                               // Original local scale
    Vector3 targetScale;                                                                 // Target local scale

    float targetHoverA;                                                                  // Target alpha for hover background
    float targetBarA;                                                                    // Target alpha for active bar

    bool inited = false;                                                                 // Whether initialization is complete

    // Hide overlays on awake
    void Awake()
    {
        if (hoverBG) hoverBG.alpha = 0f;
        if (activeBar) activeBar.alpha = 0f;
    }

    // Cache original transform values after one frame delay
    IEnumerator Start()
    {
        yield return null;

        originalPos = transform.localPosition;
        targetPos = originalPos;

        originalScale = transform.localScale;
        targetScale = originalScale;

        targetHoverA = 0f;
        targetBarA = 0f;

        transform.localPosition = targetPos;
        transform.localScale = targetScale;

        inited = true;
    }

    // Smoothly interpolate position, scale and alpha each frame
    void Update()
    {
        if (!inited) return;

        float dt = Time.unscaledDeltaTime;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, dt * speed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, dt * speed);

        if (hoverBG) hoverBG.alpha = Mathf.Lerp(hoverBG.alpha, targetHoverA, dt * speed);
        if (activeBar) activeBar.alpha = Mathf.Lerp(activeBar.alpha, targetBarA, dt * speed);
    }

    // Reset visual state when disabled
    void OnDisable()
    {
        if (!inited) return;
        ResetVisualStateImmediate();
    }

    // Set hover targets when pointer enters
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!inited) return;

        targetPos = originalPos + new Vector3(moveX, 0f, 0f);
        targetScale = originalScale * scale;

        targetHoverA = hoverBGAlpha;
        targetBarA = 1f;
    }

    // Reset targets when pointer exits
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!inited) return;

        targetPos = originalPos;
        targetScale = originalScale;

        targetHoverA = 0f;
        targetBarA = 0f;
    }

    // Immediately reset all visual properties to their original values
    private void ResetVisualStateImmediate()
    {
        targetPos = originalPos;
        targetScale = originalScale;
        targetHoverA = 0f;
        targetBarA = 0f;

        transform.localPosition = originalPos;
        transform.localScale = originalScale;
        if (hoverBG) hoverBG.alpha = 0f;
        if (activeBar) activeBar.alpha = 0f;
    }
}
