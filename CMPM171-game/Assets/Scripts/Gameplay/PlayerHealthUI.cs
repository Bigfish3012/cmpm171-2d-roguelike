using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows HP as N separate icons. One icon per HP; each hit hides one. Spawns from prefab, uses container children, or creates simple quads.
public class PlayerHealthUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private PlayerHealth source;

    [Header("Icons")]
    [SerializeField] private RectTransform container;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Sprite iconSprite;

    private List<GameObject> icons = new List<GameObject>();

    void Start()
    {
        if (source == null)
            source = FindFirstObjectByType<PlayerHealth>();
        if (source == null) return;

        if (container == null || !container.gameObject.activeInHierarchy)
            container = transform as RectTransform;
        if (container == null) return;

        if (iconPrefab != null)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
            for (int i = 0; i < source.MaxHealth; i++)
            {
                var go = Instantiate(iconPrefab, container);
                go.SetActive(true);
                icons.Add(go);
            }
        }
        else if (container.childCount >= source.MaxHealth)
        {
            for (int i = 0; i < source.MaxHealth; i++)
                icons.Add(container.GetChild(i).gameObject);
        }
        else
        {
            for (int i = 0; i < source.MaxHealth; i++)
                icons.Add(CreateDefaultIcon(i));
        }

        source.OnHealthChanged += UpdateIcons;
        UpdateIcons(source.CurrentHealth, source.MaxHealth);
    }

    GameObject CreateDefaultIcon(int index)
    {
        var go = new GameObject("Heart_" + index);
        go.transform.SetParent(container, false);
        go.transform.localScale = Vector3.one;
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(32f, 32f);
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(index * 36f, 0f);
        var img = go.AddComponent<Image>();
        if (iconSprite != null)
        {
            img.sprite = iconSprite;
            img.color = Color.white;
        }
        else
            img.color = new Color(1f, 0.25f, 0.25f);
        img.raycastTarget = false;
        return go;
    }

    void OnDestroy()
    {
        if (source != null)
            source.OnHealthChanged -= UpdateIcons;
    }

    void UpdateIcons(int current, int _)
    {
        for (int i = 0; i < icons.Count; i++)
            icons[i].SetActive(i < current);
    }
}
