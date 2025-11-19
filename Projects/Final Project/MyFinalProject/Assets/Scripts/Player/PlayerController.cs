using UnityEngine;

/// <summary>
/// Handles player movement and animation
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;

    private Rigidbody2D playerRb;
    private Vector2 movementInput;
    private Animator animator;
    private FarmingController farmingController;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        farmingController = FindFirstObjectByType<FarmingController>();
    }

    void Update()
    {
        // Don't allow movement if seed selection bar is expanded OR inventory is open
        bool canMove = true;

        // Block if seed bar is expanded
        if (SeedSelectionBar.Instance != null && SeedSelectionBar.Instance.IsExpanded)
        {
            canMove = false;
            movementInput = Vector2.zero;
        }
        // Block if inventory is open
        else if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen)
        {
            canMove = false;
            movementInput = Vector2.zero;
        }
        // Block movement if S key is being pressed (seed bar toggle key)
        else if (Input.GetKey(KeyCode.S) && SeedSelectionBar.Instance != null)
        {
            canMove = false;
            movementInput = Vector2.zero;
        }

        // Get input in Update (only if allowed to move)
        if (canMove)
        {
            movementInput.x = Input.GetAxisRaw("Horizontal");
            movementInput.y = Input.GetAxisRaw("Vertical");
            movementInput.Normalize();
        }

        // Check if farming action is locking facing direction
        bool facingIsLocked = farmingController != null && farmingController.IsFacingLocked;

        if (facingIsLocked)
        {
            // Don't update animator parameters - farming action is controlling facing
            // Still allow movement but maintain the locked facing direction
            bool isWalking = (movementInput.x != 0 || movementInput.y != 0);
            animator.SetBool("isWalking", isWalking);

            // Keep the locked facing direction from farming controller
            Vector2 lockedDirection = farmingController.LockedFacingDirection;
            animator.SetFloat("LastInputX", lockedDirection.x);
            animator.SetFloat("LastInputY", lockedDirection.y);
            animator.SetFloat("InputX", lockedDirection.x);
            animator.SetFloat("InputY", lockedDirection.y);
        }
        else
        {
            // Normal movement control - player input controls facing
            if (movementInput.x != 0 || movementInput.y != 0)
            {
                // Update last input direction for idle animations
                animator.SetFloat("LastInputX", movementInput.x);
                animator.SetFloat("LastInputY", movementInput.y);
                animator.SetFloat("InputX", movementInput.x);
                animator.SetFloat("InputY", movementInput.y);
                animator.SetBool("isWalking", true);
            }
            else
            {
                animator.SetFloat("InputX", animator.GetFloat("LastInputX"));
                animator.SetFloat("InputY", animator.GetFloat("LastInputY"));
                animator.SetBool("isWalking", false);
            }
        }
    }

    void FixedUpdate()
    {
        // Apply velocity in FixedUpdate for physics
        playerRb.linearVelocity = movementInput * moveSpeed;
    }
}