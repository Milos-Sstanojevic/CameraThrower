using UnityEngine;

public class PlayerGravityController : MonoBehaviour
{
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float fallingGravityScale;
    [SerializeField] private float jumpingGravityScale;
    [SerializeField] private float wallSlideGravityScale;
    [SerializeField] private float wallJumpGravityScale;

    private PlayerJumpController playerJumpController;
    private Rigidbody2D playerRb;
    private bool stopGravity;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerJumpController = GetComponent<PlayerJumpController>();
    }

    private void FixedUpdate()
    {
        RealisticJumpGravity();
    }

    private void RealisticJumpGravity()
    {
        if (stopGravity || playerJumpController.IsPlayerWallJumping())
            return;

        if (playerRb.velocity.y < 0)
            playerRb.gravityScale = fallingGravityScale;
        else if (playerRb.velocity.y > 0 && playerJumpController.GetJumpInput() == 0)
            playerRb.gravityScale = lowJumpMultiplier;
        else
            playerRb.gravityScale = jumpingGravityScale;
    }

    public void StopGravity() => stopGravity = true;
    public void StartGravity() => stopGravity = false;
}
