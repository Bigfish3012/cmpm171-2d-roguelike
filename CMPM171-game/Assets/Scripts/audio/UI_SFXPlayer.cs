using UnityEngine;

public class UISFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickClip;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void PlayClick()
    {
        if (clickClip == null || audioSource == null) return;
        audioSource.PlayOneShot(clickClip, volume);
    }
}
