using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BashingObject : MonoBehaviour
{
    [SerializeField] private float lerpTime;
    [SerializeField] private float bashForce;
    [SerializeField] private float timeToBash;
    [SerializeField] private float postBashGravityScale;
    [SerializeField] private float postBashControlTime;

    private PlayerController player;
    private Rigidbody2D playerRb;
    private bool bashPressed;
    private bool canBash;

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.started && canBash)
        {
            bashPressed = true;
            Dash();
        }
    }

    private void Dash()
    {
        Vector2 bashTarget = Vector2.zero;

        StopCoroutine(BashCoroutine(bashTarget));
        StopCoroutine(BashWindowCoroutine());

        Time.timeScale = Constants.StartTime;
        player.transform.position = transform.position;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 bashDirection = (mousePosition - (Vector2)player.transform.position).normalized;
        bashTarget = (Vector2)player.transform.position + bashDirection * bashForce;

        StartCoroutine(BashCoroutine(bashTarget));
        ResetBashState();
    }

    private IEnumerator BashCoroutine(Vector2 targetPosition)
    {
        player.GetComponent<PlayerMovementController>().EnableMovement();
        player.GetComponent<PlayerJumpController>().EnableJumping();

        playerRb.gravityScale = postBashGravityScale;
        float elapsedTime = 0;
        Vector2 startingPosition = player.transform.position;

        while (elapsedTime < lerpTime)
        {
            player.transform.position = Vector2.Lerp(startingPosition, targetPosition, elapsedTime / lerpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPosition;

        yield return new WaitForSeconds(postBashControlTime);

        player.GetComponent<PlayerGravityController>().StartGravity();
    }

    private void ResetBashState()
    {
        canBash = false;
        bashPressed = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() == null || (player = collision.GetComponent<PlayerController>()) == null)
            return;

        Time.timeScale = Constants.StopTime;
        playerRb = player.GetComponent<Rigidbody2D>();
        playerRb.velocity = Vector2.zero;

        DisablePlayerMovement();

        canBash = true;

        StartCoroutine(BashWindowCoroutine());
    }

    private void DisablePlayerMovement()
    {
        player.GetComponent<PlayerMovementController>().DisableMovement();
        player.GetComponent<PlayerJumpController>().DisableJumping();
        player.GetComponent<PlayerGravityController>().StopGravity();
    }

    private IEnumerator BashWindowCoroutine()
    {
        yield return new WaitForSecondsRealtime(timeToBash);
        ResetBashState();
        if (bashPressed)            //??????????????????
            yield return null;

        Time.timeScale = Constants.StartTime;
        EnablePlayerMovement();
    }

    private void EnablePlayerMovement()
    {
        player.GetComponent<PlayerMovementController>().EnableMovement();
        player.GetComponent<PlayerJumpController>().EnableJumping();
        player.GetComponent<PlayerGravityController>().StartGravity();
    }
}
