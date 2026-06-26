using UnityEngine;

public class Animation : MonoBehaviour
{
    Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void SetBool(bool b)
    {
        animator.SetBool("hovering", b);
    }
}
