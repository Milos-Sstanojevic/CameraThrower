using System.Collections;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
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

    private void Awake()
    {
        EventManager.Instance.SubscribeToOnBasing(StopCoroutines);
    }

    private void StopCoroutines()
    {
        StopCoroutine(BashCoroutine(Vector2.zero));
        StopCoroutine(BashWindowCoroutine());
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.started && canBash)
        {
            bashPressed = true;
            Bash();
        }
    }

    private void Bash()
    {
        // StopAllCoroutines();
        EventManager.Instance.OnBashing();

        Time.timeScale = Constants.StartTime;
        player.transform.position = transform.position;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 bashDirection = (mousePosition - (Vector2)player.transform.position).normalized;
        Vector2 bashTarget = (Vector2)player.transform.position + bashDirection * bashForce;

        playerRb.velocity = Vector2.zero;
        StartCoroutine(BashCoroutine(bashTarget));
        ResetBashState();
    }

    private IEnumerator BashCoroutine(Vector2 targetPosition)
    {
        player.GetComponent<PlayerMovementController>().EnableMovement();

        player.GetComponent<PlayerDashingController>().ForbidDashingWhileBashing();

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
        player.GetComponent<PlayerDashingController>().AllowDashAfterBashing();

        yield return new WaitForSeconds(postBashControlTime);

        player.GetComponent<PlayerJumpController>().EnableJumping();
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
        if (bashPressed)
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

    private void OnDisable()
    {
        EventManager.Instance.UnsubscribeFromOnBashing(StopCoroutines);
    }
}
