using UnityEngine;

public class Chest : MonoBehaviour
{
    public int bonusScore = 100;
    public bool isOpen = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public int OpenChest()
    {
        if (isOpen) return 0;

        isOpen = true;
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }
        return bonusScore;
    }
}