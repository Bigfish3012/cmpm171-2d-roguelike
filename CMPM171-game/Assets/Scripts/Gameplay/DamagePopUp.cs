using UnityEngine;
using TMPro;

// Spawns at world position, floats up and fades out. Used for damage numbers on enemies.
public class DamagePopUp : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private float horizontalOffsetRange = 0.3f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float normalFontSize = 36f;
    [SerializeField] private Color critColor = Color.red;
    [SerializeField] private float critFontSize = 40f;

    private TextMeshProUGUI textMesh;
    private float timer;
    private Vector3 startPos;

    public void Init(int damage, Vector3 worldPosition, bool isCrit = false)
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            if(isCrit)
            {
                textMesh.color = critColor;
                textMesh.fontSize = critFontSize;
            } else {
                textMesh.color = normalColor;
                textMesh.fontSize = normalFontSize;
            }
        }
        Vector3 offset = Vector3.up * 0.5f + Vector3.right * Random.Range(-horizontalOffsetRange, horizontalOffsetRange);
        transform.position = worldPosition + offset;
        startPos = transform.position;
        timer = lifetime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        // Float upward
        transform.position = startPos + Vector3.up * (floatSpeed * (lifetime - timer));
        // Fade out
        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = timer / lifetime;
            textMesh.color = c;
        }
    }
}
