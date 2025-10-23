using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;

    private Rigidbody2D playerRb;
    private Vector2 movementInput;
    private Animator animator;
    public bool isWalking = false;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Get input in Update
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput.Normalize();

        // Check if player is moving
        if (movementInput.x != 0 || movementInput.y != 0)
        {
            isWalking = true;
            // Update last input direction for idle animations
            animator.SetFloat("LastInputX", movementInput.x);
            animator.SetFloat("LastInputY", movementInput.y);
            animator.SetFloat("InputX", movementInput.x);
            animator.SetFloat("InputY", movementInput.y);
        }
        else
        {
            isWalking = false;
            animator.SetFloat("InputX", animator.GetFloat("LastInputX"));
            animator.SetFloat("InputY", animator.GetFloat("LastInputY"));
        }

        animator.SetBool("isWalking", isWalking);        
    }

    void FixedUpdate()
    {
        // Apply velocity in FixedUpdate for physics
        playerRb.linearVelocity = movementInput * moveSpeed;
    }
}