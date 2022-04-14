using UnityEngine;

public class ControlDoor : MonoBehaviour
{
    private Activatable activatable;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        activatable = GetComponent<Activatable>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isOpened", activatable.isActivated);
    }
}
