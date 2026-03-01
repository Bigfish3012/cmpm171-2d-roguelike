using UnityEngine;

public class UIShineSweep : MonoBehaviour
{
    public RectTransform shine;
    public RectTransform container;

    public float speed = 600f;
    public float interval = 1.5f;
    public bool useUnscaledTime = true;

    float timer;

    void Awake()
    {
        if (shine == null) shine = transform as RectTransform;
        if (container == null) container = shine.parent as RectTransform;
        ResetToLeft();
        timer = interval;
    }

    void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        timer -= dt;
        if (timer > 0f) return;

        Vector2 pos = shine.anchoredPosition;
        pos.x += speed * dt;
        shine.anchoredPosition = pos;

        float rightEdge = container.rect.width + shine.rect.width;
        if (pos.x > rightEdge)
        {
            ResetToLeft();
            timer = interval;
        }
    }

    void ResetToLeft()
    {
        float startX = -shine.rect.width;
        shine.anchoredPosition = new Vector2(startX, 0f);
    }
}