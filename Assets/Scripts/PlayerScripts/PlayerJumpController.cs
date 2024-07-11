using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJumpController : MonoBehaviour
{
    private const float RadiusOfOverallWallCircle = 0.5f;
    private const int JumpLeft = -1;
    private const int JumpRight = 1;

    [SerializeField] private float jumpForce;
    [SerializeField] private Transform foot;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float raycastDistance;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float bufferJumpDistance;
    [SerializeField] private float wallJumpForceMultiplierY;
    [SerializeField] private float wallJumpVelocityX;
    [SerializeField] private float halfwayDuration;
    [SerializeField] private float wallJumpGravityScale;
    [SerializeField] private float slidingSpeed;
    [SerializeField] private float wallSlideGravityScale;

    private PlayerMovementController playerMovementController;
    private Rigidbody2D playerRb;
    private float jumpInput;
    private float coyoteTimeCounter;
    private bool isWallJumping;
    private bool bufferedJump;
    private bool jumpingDisabled;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerMovementController = GetComponent<PlayerMovementController>();

        EventManager.Instance.SubscribeToOnBasing(StopCoroutines);
    }

    private void StopCoroutines()
    {
        StopCoroutine(WallJumpCoroutine());
    }

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
        playerRb.velocity = Vector2.zero;
        float initialDirection = GetComponent<PlayerController>().IsFacingRight() ? JumpLeft : JumpRight;
        float velocityX = wallJumpVelocityX * initialDirection;
        playerRb.velocity = new Vector2(velocityX, jumpForce * wallJumpForceMultiplierY);

        yield return new WaitForSeconds(halfwayDuration);
        float currentVelocityX = playerRb.velocity.x;
        playerRb.velocity = new Vector2(0, playerRb.velocity.y);

        if (jumpInput > 0)
            playerRb.velocity = new Vector2(-currentVelocityX, playerRb.velocity.y);

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

    private void Update()
    {
        BufferedJump();
        CoyoteJump();
        SlideWall();
    }

    private void BufferedJump()
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

    private void SlideWall()
    {
        if (IsWallSliding() && !isWallJumping)
        {
            playerRb.gravityScale = wallSlideGravityScale;
            playerRb.velocity = new Vector2(playerRb.velocity.x, slidingSpeed);
        }
    }

    private bool IsWallSliding() => playerMovementController.GetMovementInput().x != 0 && IsTouchingWall() && !CheckIfGrounded();

    private bool IsTouchingWall() => Physics2D.OverlapCircle(transform.position, RadiusOfOverallWallCircle, wallMask);

    public void DisableJumping() => jumpingDisabled = true;
    public void EnableJumping() => jumpingDisabled = false;
    public float GetJumpInput() => jumpInput;
    public bool IsPlayerWallJumping() => isWallJumping;

    private void OnDisable()
    {
        EventManager.Instance.UnsubscribeFromOnBashing(StopCoroutines);
    }
}
