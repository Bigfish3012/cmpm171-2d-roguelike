using UnityEngine;

public class UISFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickClip;
    [Range(0f, 2f)][SerializeField] private float volume = 2f;

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
