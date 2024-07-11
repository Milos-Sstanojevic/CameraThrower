using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Vector2 maximumVelocity;
    [SerializeField] private Vector2 minimumVelocity;

    private bool isFacingRight = true;
    private Rigidbody2D playerRb;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        FlipPlayer();
    }

    private void FixedUpdate()
    {
        playerRb.velocity = new Vector2(Mathf.Clamp(playerRb.velocity.x, minimumVelocity.x, maximumVelocity.x), Mathf.Clamp(playerRb.velocity.y, minimumVelocity.y, maximumVelocity.y));
    }

    private void FlipPlayer()
    {
        if ((!isFacingRight || GetComponent<PlayerMovementController>().GetMovementInput().x >= 0) && (isFacingRight || GetComponent<PlayerMovementController>().GetMovementInput().x <= 0))
            return;

        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x = -localScale.x;
        transform.localScale = localScale;
    }

    public bool IsFacingRight() => isFacingRight;
}
