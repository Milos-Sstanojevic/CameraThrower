using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static Vector2 SizeOfOverlapCollider = new Vector2(0.25f, 0.25f);
    private const float AngleOfOverlapCollider = 0;
    private const float DistanceFromHorizontalBounds = 0.5f;
    private const float DistanceFromVerticalBounds = 1f;

    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform foot;
    [SerializeField] private Transform head;
    [SerializeField] private Transform hand;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask boundsMask;
    [SerializeField] private float raycastDistance;
    [SerializeField] private Transform leftBound;
    [SerializeField] private Transform rightBound;
    [SerializeField] private Transform topBound;
    [SerializeField] private Transform bottomBound;
    [SerializeField] private ThrowingCameraController throwingCamera;
    [SerializeField] private float wallSlidingSpeed;
    [SerializeField] private float wallJumpDuration;
    [SerializeField] private Vector2 wallJumpForce;
    [SerializeField] private float wallJumpingTime = 0.2f;
    [SerializeField] private float fallMultiplier = 1.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingCounter;
    private bool isSliding;
    private Rigidbody2D playerRb;
    private Vector2 movementInput;
    private float jumpInput;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    [SerializeField] private float slideAcceleration;
    private bool isFacingRight = true;
    [SerializeField] private float fallingGravityScale;
    [SerializeField] private float jumpingGravityScale;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        speed = 8;
    }

    //Callback for player movement from new input system
    public void OnMoveLeftOrRigth(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    //Callback for player jumping from new input system
    public void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValue<float>();

        if (context.started)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        if (context.canceled)
        {
            jumpBufferCounter = 0f;
        }
    }

    private void StopWallJump()
    {
        isWallJumping = false;
    }

    private void Jump()
    {
        playerRb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
    }

    //Callback for player interaction with object from new input system
    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        if (throwingCamera.IsCameraPickedUp())
            throwingCamera.ReleaseCamera();
        else
            InteractWithObject();
    }

    private void InteractWithObject()
    {
        if (Physics2D.OverlapBox(throwingCamera.transform.position, SizeOfOverlapCollider, AngleOfOverlapCollider) == GetComponent<Collider2D>())
            throwingCamera.PickUpCamera(transform);
    }

    private void Update()
    {
        RealisticJump();

        CoyoteJump();
        BufferedJump();
        WallJump();
        SlideWall();
        if (!isWallJumping)
            FlipPlayer();

        KeepPlayerInBounds();

    }

    private void CoyoteJump()
    {
        if (CheckIfGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    private void BufferedJump()
    {
        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!isWallJumping)
            MovePlayer();
    }


    private void WallJump()
    {
        if (isSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJump));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (jumpInput != 0 && wallJumpingCounter > 0f && jumpBufferCounter > 0f)
        {
            isWallJumping = true;
            playerRb.AddForce(new Vector2(wallJumpingDirection * wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
                FlipPlayer();

            Invoke(nameof(StopWallJump), wallJumpDuration);
        }
    }

    private void FlipPlayer()
    {
        if (isFacingRight && movementInput.x < 0 || !isFacingRight && movementInput.x > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x = -localScale.x;
            transform.localScale = localScale;
        }
    }

    private void Slide()
    {
        wallJumpingDirection = -transform.localScale.x;
        float speedDif = wallSlidingSpeed - playerRb.velocity.y;
        float movement = speedDif * slideAcceleration;

        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        playerRb.AddForce(movement * Vector2.up);
    }

    private void MovePlayer()
    {
        if (isSliding)
            transform.Translate(movementInput.x * speed * Time.deltaTime * Vector2.right);
        else
        {
            playerRb.velocity = new Vector2(movementInput.x * speed, Mathf.Clamp(playerRb.velocity.y, -10f, float.MaxValue));
        }
    }

    private bool IsWalled() => Physics2D.OverlapCircle(hand.position, 0.2f, wallMask);

    private void SlideWall()
    {
        if (IsWalled() && !CheckIfGrounded() && movementInput.x != 0)
        {
            isSliding = true;
            Slide();
        }
        else
            isSliding = false;
    }

    private void RealisticJump()
    {
        if (playerRb.velocity.y < 0)
            playerRb.gravityScale = fallingGravityScale;
        else if (playerRb.velocity.y > 0 && jumpInput == 0)
            playerRb.gravityScale = lowJumpMultiplier;
        else
            playerRb.gravityScale = jumpingGravityScale;
    }

    private bool CheckIfGrounded() => Physics2D.Raycast(foot.position, Vector2.down, raycastDistance, groundMask);



    private void KeepPlayerInBounds()
    {
        Collider2D checkingCollider = Physics2D.OverlapCapsule(new Vector2(leftBound.position.x + DistanceFromHorizontalBounds, transform.position.y), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, boundsMask) && checkingCollider == null)
            transform.position = new Vector2(leftBound.position.x + DistanceFromHorizontalBounds, transform.position.y);

        checkingCollider = Physics2D.OverlapCapsule(new Vector2(rightBound.position.x - DistanceFromHorizontalBounds, transform.position.y), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, boundsMask) && checkingCollider == null)
            transform.position = new Vector2(rightBound.position.x - DistanceFromHorizontalBounds, transform.position.y);

        checkingCollider = Physics2D.OverlapCapsule(new Vector2(transform.position.x, bottomBound.position.y + DistanceFromVerticalBounds), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(head.position, Vector2.up, raycastDistance, boundsMask) && checkingCollider == null)
            transform.position = new Vector2(transform.position.x, bottomBound.position.y + DistanceFromVerticalBounds);

        checkingCollider = Physics2D.OverlapCapsule(new Vector2(transform.position.x, topBound.position.y - DistanceFromVerticalBounds), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(foot.position, Vector2.down, raycastDistance, boundsMask) && checkingCollider == null)
            transform.position = new Vector2(transform.position.x, topBound.position.y - DistanceFromVerticalBounds);
    }


}
