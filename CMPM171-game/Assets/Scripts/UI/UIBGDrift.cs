using UnityEngine;

public class UIBGDrift : MonoBehaviour
{
    public RectTransform target;
    public Vector2 driftSpeed = new Vector2(8f, 4f);
    public float bobScale = 0.015f;
    public float bobSpeed = 0.25f;
    public bool useUnscaledTime = true;

    Vector2 startPos;
    Vector3 startScale;
    float t;

    void Awake()
    {
        if (target == null) target = transform as RectTransform;
        startPos = target.anchoredPosition;
        startScale = target.localScale;
    }

    void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        t += dt;

        Vector2 p = startPos + driftSpeed * t;
        p.x = Mathf.Repeat(p.x, 200f) - 100f;
        p.y = Mathf.Repeat(p.y, 200f) - 100f;
        target.anchoredPosition = p;

        float s = 1f + Mathf.Sin(t * Mathf.PI * 2f * bobSpeed) * bobScale;
        target.localScale = startScale * s;
    }
}