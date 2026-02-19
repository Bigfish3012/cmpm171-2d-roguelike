using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuItemHoverSmooth : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Refs")]
    public CanvasGroup hoverBG;
    public CanvasGroup activeBar;

    [Header("Tuning")]
    public float hoverBGAlpha = 0.15f;
    public float moveX = 14f;
    public float scale = 1.03f;
    public float speed = 12f;

    Vector3 originalPos;
    Vector3 targetPos;

    Vector3 originalScale;
    Vector3 targetScale;

    float targetHoverA;
    float targetBarA;

    bool inited = false;

    void Awake()
    {
        if (hoverBG) hoverBG.alpha = 0f;
        if (activeBar) activeBar.alpha = 0f;
    }

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

    void Update()
    {
        if (!inited) return;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * speed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);

        if (hoverBG) hoverBG.alpha = Mathf.Lerp(hoverBG.alpha, targetHoverA, Time.deltaTime * speed);
        if (activeBar) activeBar.alpha = Mathf.Lerp(activeBar.alpha, targetBarA, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!inited) return;

        targetPos = originalPos + new Vector3(moveX, 0f, 0f);
        targetScale = originalScale * scale;

        targetHoverA = hoverBGAlpha;
        targetBarA = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!inited) return;

        targetPos = originalPos;
        targetScale = originalScale;

        targetHoverA = 0f;
        targetBarA = 0f;
    }
}
