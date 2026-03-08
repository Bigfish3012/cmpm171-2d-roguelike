using UnityEngine;

public class UIShineSweep_FromAccent : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform shine;        // ShineBar
    public RectTransform container;    // ShineLayer (masked)
    public RectTransform accentLine;   // AccentLine (left vertical bar)

    [Header("Motion")]
    public float speed = 600f;
    public float interval = 1.2f;
    public bool useUnscaledTime = true;

    [Header("Offsets")]
    public float startPadding = 8f;    // extra px after accent line
    public float endPadding = 40f;     // extra px after leaving right side

    float timer;

    void Awake()
    {
        if (shine == null) shine = transform as RectTransform;
        if (container == null) container = shine.parent as RectTransform;
        ResetToStart();
        timer = interval;
    }

    void OnEnable()
    {
        ResetToStart();
        timer = interval;
    }

    void Update()
    {
        if (!shine || !container) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        timer -= dt;
        if (timer > 0f) return;

        Vector2 pos = shine.anchoredPosition;
        pos.x += speed * dt;
        shine.anchoredPosition = pos;

        float halfW = container.rect.width * 0.5f;
        float offRight = halfW + shine.rect.width + endPadding;

        if (pos.x > offRight)
        {
            ResetToStart();
            timer = interval;
        }
    }

    void ResetToStart()
    {
        float halfW = container.rect.width * 0.5f;

        float accentW = 0f;
        if (accentLine != null) accentW = accentLine.rect.width;

        // start at left outside, then shift right by accent width + padding
        float startX = (-halfW - shine.rect.width) + accentW + startPadding;
        shine.anchoredPosition = new Vector2(startX, 0f);
    }
}