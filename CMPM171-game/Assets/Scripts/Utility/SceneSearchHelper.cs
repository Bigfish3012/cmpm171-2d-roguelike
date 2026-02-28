using TMPro;
using UnityEngine;

/// <summary>
/// Shared helpers for finding scene objects by name at runtime.
/// Used by EnemySpawner, WaveFlowController, and any future scripts
/// that need to auto-bind references.
/// </summary>
public static class SceneSearchHelper
{
    /// <summary>Find an active-scene GameObject by exact name.</summary>
    public static GameObject FindSceneObjectByName(string targetName)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject go = allObjects[i];
            if (go == null) continue;
            if (!go.scene.IsValid()) continue;
            if (go.name == targetName) return go;
        }
        return null;
    }

    /// <summary>Find an active-scene TextMeshProUGUI component by GameObject name.</summary>
    public static TextMeshProUGUI FindSceneTMPByName(string targetName)
    {
        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        for (int i = 0; i < allTexts.Length; i++)
        {
            TextMeshProUGUI text = allTexts[i];
            if (text == null) continue;
            if (!text.gameObject.scene.IsValid()) continue;
            if (text.name == targetName) return text;
        }
        return null;
    }
}
