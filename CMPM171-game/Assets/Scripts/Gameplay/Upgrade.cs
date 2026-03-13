using UnityEngine;

public class Upgrade : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip targetAnimation;
    private Coroutine disableRoutine;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning("Upgrade: No Animator found on this object or its children.", this);
            return;
        }

        if (targetAnimation == null)
        {
            Debug.LogWarning("Upgrade: targetAnimation is not assigned.", this);
            return;
        }

        int stateHash = Animator.StringToHash(targetAnimation.name);
        if (!animator.HasState(0, stateHash))
        {
            Debug.LogWarning($"Upgrade: State '{targetAnimation.name}' not found in Animator layer 0.", this);
            return;
        }

        animator.Play(stateHash, 0, 0f);
        disableRoutine = StartCoroutine(DisableAfterAnimation());
    }

    private System.Collections.IEnumerator DisableAfterAnimation()
    {
        yield return new WaitForSeconds(targetAnimation.length);
        disableRoutine = null;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        disableRoutine = null;
    }
}
