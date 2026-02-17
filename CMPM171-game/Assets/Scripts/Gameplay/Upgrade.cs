using UnityEngine;

public class Upgrade : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip targetAnimation;

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
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (targetAnimation == null)
            {
                Debug.LogWarning("Upgrade: targetAnimation is not assigned.", this);
                return;
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null) animator = GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                Debug.LogWarning("Upgrade: No Animator found on this object or its children.", this);
                return;
            }

            int stateHash = Animator.StringToHash(targetAnimation.name);
            if (!animator.HasState(0, stateHash))
            {
                Debug.LogWarning($"Upgrade: State '{targetAnimation.name}' not found in Animator layer 0.", this);
                return;
            }

            animator.Play(targetAnimation.name, 0, 0f);
        }
    }
}
