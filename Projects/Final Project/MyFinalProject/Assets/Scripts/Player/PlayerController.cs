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
        Vector3 positionBefore = transform.position;

        // Don't allow movement if seed selection bar is expanded OR inventory is open OR if has just closed this frame
        bool canMove = true;

        // Block if seed bar is expanded
        if (SeedSelectionBar.Instance != null && SeedSelectionBar.Instance.IsExpanded)
        {
            canMove = false;
            movementInput = Vector2.zero; // Clear input
        }
        // Block if inventory is open
        else if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen)
        {
            canMove = false;
            movementInput = Vector2.zero; // Clear input
        }
        // Block movement if S key is being pressed (seed bar toggle key)
        else if (Input.GetKey(KeyCode.S) && SeedSelectionBar.Instance != null)
        {
            canMove = false;
            movementInput = Vector2.zero; // Clear input
        }

        // Get input in Update (only if allowed to move)
        if (canMove)
        {
            movementInput.x = Input.GetAxisRaw("Horizontal");
            movementInput.y = Input.GetAxisRaw("Vertical");
            movementInput.Normalize();
        }

        // Normal movement control - player input controls facing
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

        if (Vector3.Distance(positionBefore, transform.position) > 0.01f)
        {
            Debug.LogWarning($"[PlayerController] Position changed in Update from {positionBefore} to {transform.position}");
        }
    }

    void FixedUpdate()
    {
        Vector3 positionBefore = transform.position;

        // Apply velocity in FixedUpdate for physics
        playerRb.linearVelocity = movementInput * moveSpeed;

        if (Vector3.Distance(positionBefore, transform.position) > 0.01f)
        {
            Debug.LogWarning($"[PlayerController] Position changed in FixedUpdate from {positionBefore} to {transform.position}");
        }
    }
}