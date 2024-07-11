using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDashingController : MonoBehaviour
{
    [SerializeField] private float dashForce;
    [SerializeField] private float dashingColldown;
    [SerializeField] private float dashDuration;
    private Rigidbody2D playerRb;
    private bool canDash = true;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        EventManager.Instance.SubscribeToOnBasing(StopCoroutines);
    }

    private void StopCoroutines()
    {
        StopCoroutine(DashingColldown());
        StopCoroutine(DashingCoroutine());
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
            Dash();
    }

    private void Dash()
    {
        StartCoroutine(DashingCoroutine());

        canDash = false;
    }

    private IEnumerator DashingCoroutine()
    {
        GetComponent<PlayerMovementController>().DisableMovement();
        GetComponent<PlayerJumpController>().DisableJumping();
        GetComponent<PlayerGravityController>().StopGravity();

        Vector2 dashDirection = GetComponent<PlayerController>().IsFacingRight() ? Vector2.right : Vector2.left;
        playerRb.gravityScale = 0;

        playerRb.velocity = new Vector2(playerRb.velocity.x, 0);

        playerRb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(dashDuration);

        playerRb.velocity = Vector2.zero;

        GetComponent<PlayerMovementController>().EnableMovement();
        GetComponent<PlayerJumpController>().EnableJumping();
        GetComponent<PlayerGravityController>().StartGravity();

        StartCoroutine(DashingColldown());
    }

    private IEnumerator DashingColldown()
    {
        yield return new WaitForSeconds(dashingColldown);

        canDash = true;
    }

    private void OnDisable()
    {
        EventManager.Instance.UnsubscribeFromOnBashing(StopCoroutines);
    }

    public void AllowDashAfterBashing()
    {
        canDash = true;
    }

    public void ForbidDashingWhileBashing()
    {
        canDash = false;
    }

}
