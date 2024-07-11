using UnityEngine;

public class PlayerBoundsController : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform foot;
    [SerializeField] private float raycastDistance;
    [SerializeField] private LayerMask boundsMask;
    [SerializeField] private Transform leftBound;
    [SerializeField] private Transform rightBound;
    [SerializeField] private Transform topBound;
    [SerializeField] private Transform bottomBound;
    [SerializeField] private float distanceFromHorizontalBounds = 0.5f;
    [SerializeField] private float distanceFromVerticalBounds = 1f;
    [SerializeField] private Vector2 sizeOfOverlapCollider = new Vector2(0.25f, 0.25f);
    [SerializeField] private float angleOfOverlapCollider = 0;

    private void Update()
    {
        KeepPlayerInBounds();
    }

    private void KeepPlayerInBounds()
    {
        CheckAndTeleport(Vector2.right, new Vector2(leftBound.position.x + distanceFromHorizontalBounds, transform.position.y), transform.position);
        CheckAndTeleport(Vector2.left, new Vector2(rightBound.position.x - distanceFromHorizontalBounds, transform.position.y), transform.position);
        CheckAndTeleport(Vector2.up, new Vector2(transform.position.x, bottomBound.position.y + distanceFromVerticalBounds), head.position);
        CheckAndTeleport(Vector2.down, new Vector2(transform.position.x, topBound.position.y - distanceFromVerticalBounds), foot.position);
    }

    private void CheckAndTeleport(Vector2 direction, Vector2 targetPosition, Vector2 positionForRaycast)
    {
        Vector2 overlapPosition = targetPosition;
        Collider2D colliderAfterTeleportation = Physics2D.OverlapCapsule(overlapPosition, sizeOfOverlapCollider, CapsuleDirection2D.Vertical, angleOfOverlapCollider);

        if (Physics2D.Raycast(positionForRaycast, direction, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = targetPosition;
    }
}
