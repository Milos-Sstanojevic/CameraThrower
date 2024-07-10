using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashingObject : MonoBehaviour
{
    [SerializeField] private float lerpTime;
    [SerializeField] private float dashForce;
    [SerializeField] private float timeToDash;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask dasherLayer;
    private PlayerController player;
    private Rigidbody2D playerRb;
    private float startingGravity;
    private bool dashPressed;
    private bool collidedWithDasher;
    private bool canDash;
    private bool canTimeStop = true;

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            dashPressed = true;
            Dash();
        }
    }

    private void Dash()
    {
        StopCoroutine(DashWindowCoroutine());
        Time.timeScale = 1;
        player.transform.position = transform.position;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dashDirection = (mousePosition - (Vector2)player.transform.position).normalized;
        Vector2 dashTarget = (Vector2)player.transform.position + dashDirection * dashForce;

        StartCoroutine(DashCoroutine(dashTarget));
        ResetDashState();
    }

    private IEnumerator DashCoroutine(Vector2 targetPosition)
    {
        float elapsedTime = 0;
        Vector2 startingPosition = player.transform.position;

        while (elapsedTime < lerpTime)
        {
            player.transform.position = Vector2.Lerp(startingPosition, targetPosition, elapsedTime / lerpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPosition;
        playerRb.gravityScale = 3;
        player.EnableMovementJumpingAndGravityChanges();
    }

    private void ResetDashState()
    {
        canDash = false;
        dashPressed = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                Time.timeScale = 0;
                playerRb = player.GetComponent<Rigidbody2D>();
                playerRb.velocity = Vector2.zero;
                player.DisableMovementJumpingAndGravityChanges();
                canDash = true;
                StartCoroutine(DashWindowCoroutine());
            }
        }
    }

    private IEnumerator DashWindowCoroutine()
    {
        yield return new WaitForSecondsRealtime(timeToDash);
        if (!dashPressed)
        {
            Time.timeScale = 1;
            player.EnableMovementJumpingAndGravityChanges();
            ResetDashState();
        }
    }


}
