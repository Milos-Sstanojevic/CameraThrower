using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private const int JumpLeft = -1;
    private const int JumpRight = 1;
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
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float fallingGravityScale;
    [SerializeField] private float jumpingGravityScale;
    [SerializeField] private float coyoteTime = 0.2f;

    private Rigidbody2D playerRb;
    private Vector2 movementInput;
    private float jumpInput;
    private float coyoteTimeCounter;
    private bool isFacingRight = true;

    [SerializeField] private float bufferJumpDistance;
    private bool bufferedJump;
    [SerializeField] private float slidingSpeed;
    [SerializeField] private float wallJumpForceMultiplierY;
    [SerializeField] private float wallJumpVelocityX;
    [SerializeField] private float halfwayDuration;
    private bool isWallJumping;
    [SerializeField] private float wallSlideGravityScale;
    [SerializeField] private float wallJumpGravityScale;
    private bool movementDisabled;
    private bool jumpingDisabled;
    private bool stopGravity;

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

        if (!context.started)
            return;

        if (IsWallSliding())
        {
            StartCoroutine(WallJumpCoroutine());
        }
        else if (CheckIfGrounded() || coyoteTimeCounter > 0)
        {
            ResetVelocityYToZero();
            Jump();
        }
        else if (IsPlayerCloseToGround())
        {
            bufferedJump = true;
        }
    }

    private IEnumerator WallJumpCoroutine()
    {
        isWallJumping = true;
        playerRb.gravityScale = wallJumpGravityScale;
        float initialDirection = isFacingRight ? JumpLeft : JumpRight;

        float velocityX = wallJumpVelocityX * initialDirection;
        playerRb.velocity = new Vector2(velocityX, jumpForce * wallJumpForceMultiplierY);

        yield return new WaitForSeconds(halfwayDuration);

        if (jumpInput > 0)
        {
            playerRb.velocity = new Vector2(-velocityX, playerRb.velocity.y);
        }
        isWallJumping = false;
    }

    private bool CheckIfGrounded() => Physics2D.Raycast(foot.position, Vector2.down, raycastDistance, groundMask);

    private void ResetVelocityYToZero()
    {
        playerRb.velocity = new Vector2(playerRb.velocity.x, 0);
    }

    private bool IsPlayerCloseToGround() => Physics2D.OverlapCircle(foot.position, bufferJumpDistance, groundMask);

    private void Jump()
    {
        if (jumpingDisabled)
            return;

        playerRb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
        bufferedJump = false;
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
        FlipPlayer();

        BufferedJump();

        CoyoteJump();

        KeepPlayerInBounds();
    }

    public void BufferedJump()
    {
        if (bufferedJump && CheckIfGrounded())
            Jump();
    }



    private void CoyoteJump()
    {
        if (CheckIfGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!movementDisabled)
            MovePlayer();

        SlideWall();

        RealisticJumpGravity();
    }

    private void MovePlayer()
    {
        transform.Translate(speed * Time.deltaTime * movementInput.x * Vector2.right);
    }

    private void SlideWall()
    {
        if (IsWallSliding() && !isWallJumping)
        {
            playerRb.gravityScale = wallSlideGravityScale;
            playerRb.velocity = new Vector2(playerRb.velocity.x, slidingSpeed);
        }
    }

    private void RealisticJumpGravity()
    {
        if (IsWallSliding() || isWallJumping || stopGravity)
            return;

        if (playerRb.velocity.y < 0)
            playerRb.gravityScale = fallingGravityScale;
        else if (playerRb.velocity.y > 0 && jumpInput == 0)
            playerRb.gravityScale = lowJumpMultiplier;
        else
            playerRb.gravityScale = jumpingGravityScale;
    }

    private bool IsWallSliding() => movementInput.x != 0 && IsTouchingWall() && !CheckIfGrounded();

    private bool IsTouchingWall() => Physics2D.OverlapCircle(transform.position, 0.5f, wallMask);

    private void FlipPlayer()
    {
        if ((!isFacingRight || movementInput.x >= 0) && (isFacingRight || movementInput.x <= 0))
            return;

        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x = -localScale.x;
        transform.localScale = localScale;
    }

    private void KeepPlayerInBounds()
    {
        Collider2D colliderAfterTeleportation = Physics2D.OverlapCapsule(new Vector2(leftBound.position.x + DistanceFromHorizontalBounds, transform.position.y), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = new Vector2(leftBound.position.x + DistanceFromHorizontalBounds, transform.position.y);

        colliderAfterTeleportation = Physics2D.OverlapCapsule(new Vector2(rightBound.position.x - DistanceFromHorizontalBounds, transform.position.y), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = new Vector2(rightBound.position.x - DistanceFromHorizontalBounds, transform.position.y);

        colliderAfterTeleportation = Physics2D.OverlapCapsule(new Vector2(transform.position.x, bottomBound.position.y + DistanceFromVerticalBounds), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(head.position, Vector2.up, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = new Vector2(transform.position.x, bottomBound.position.y + DistanceFromVerticalBounds);

        colliderAfterTeleportation = Physics2D.OverlapCapsule(new Vector2(transform.position.x, topBound.position.y - DistanceFromVerticalBounds), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(foot.position, Vector2.down, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = new Vector2(transform.position.x, topBound.position.y - DistanceFromVerticalBounds);
    }


    public void DisableMovementJumpingAndGravityChanges()
    {
        movementDisabled = true;
        jumpingDisabled = true;
        stopGravity = true;
    }

    public void EnableMovementJumpingAndGravityChanges()
    {
        movementDisabled = false;
        jumpingDisabled = false;
        stopGravity = false;
    }

    public void EnableMovementAndJumping()
    {
        movementDisabled = false;
        jumpingDisabled = false;
    }

}
