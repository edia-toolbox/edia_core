using UnityEngine;

public class Indicator : MonoBehaviour
{
    private Animator animator = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Deactivate();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        animator.SetBool("Show", true);
    }

    public void Hide()
    {
        animator.SetBool("Show", false);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
