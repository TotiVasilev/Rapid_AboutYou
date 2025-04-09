using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float raycastDistance = 1f;
    public float avoidanceDistance = 1f;
    public LayerMask obstacleLayer;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Sprite upSprite, downSprite, leftSprite, rightSprite;
    public Sprite upSpriteWithBucket, downSpriteWithBucket, leftSpriteWithBucket, rightSpriteWithBucket;

    private Vector2 targetPosition;
    private bool isHoldingBucket = false;
    private bool isMoving = false;

    private enum Direction { Up, Down, Left, Right, None }
    private Direction lastDirection = Direction.None;

    void Start()
    {
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        targetPosition = transform.position;
        lastDirection = Direction.Right;
        UpdateSprite();
    }

    void Update()
    {
        UpdateFacingDirection();
        UpdateSprite();
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }

    private void MoveTowardsTarget()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, raycastDistance, obstacleLayer);

        if (hit.collider != null)
        {
            Vector2 avoidanceDirection = TryAvoidObstacle(direction);
            rb.linearVelocity = avoidanceDirection.normalized * moveSpeed;
        }
        else
        {
            if (direction.magnitude > 0.1f)
            {
                rb.linearVelocity = direction.normalized * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                isMoving = false;
            }
        }
    }

    private Vector2 TryAvoidObstacle(Vector2 direction)
    {
        Vector2 leftDirection = Quaternion.Euler(0, 0, 90) * direction;
        Vector2 rightDirection = Quaternion.Euler(0, 0, -90) * direction;

        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, leftDirection, avoidanceDistance, obstacleLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, rightDirection, avoidanceDistance, obstacleLayer);

        if (hitLeft.collider == null)
            return leftDirection;
        else if (hitRight.collider == null)
            return rightDirection;
        else
            return direction.normalized;
    }

    private void UpdateFacingDirection()
    {
        if (isMoving)
        {
            Vector2 direction = targetPosition - (Vector2)transform.position;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                lastDirection = direction.x > 0 ? Direction.Right : Direction.Left;
            else
                lastDirection = direction.y > 0 ? Direction.Up : Direction.Down;
        }
    }

    private void UpdateSprite()
    {
        if (isHoldingBucket)
        {
            switch (lastDirection)
            {
                case Direction.Right: spriteRenderer.sprite = rightSpriteWithBucket; break;
                case Direction.Left: spriteRenderer.sprite = leftSpriteWithBucket; break;
                case Direction.Up: spriteRenderer.sprite = upSpriteWithBucket; break;
                case Direction.Down: spriteRenderer.sprite = downSpriteWithBucket; break;
                default: spriteRenderer.sprite = rightSpriteWithBucket; break;
            }
        }
        else
        {
            switch (lastDirection)
            {
                case Direction.Right: spriteRenderer.sprite = rightSprite; break;
                case Direction.Left: spriteRenderer.sprite = leftSprite; break;
                case Direction.Up: spriteRenderer.sprite = upSprite; break;
                case Direction.Down: spriteRenderer.sprite = downSprite; break;
                default: spriteRenderer.sprite = rightSprite; break;
            }
        }
    }

    public void PickUpBucket()
    {
        isHoldingBucket = true;
        UpdateSprite();
    }

    public void DropBucket()
    {
        isHoldingBucket = false;
        UpdateSprite();
    }

    public void MoveTo(Vector2 target)
    {
        targetPosition = target;
        isMoving = true;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsNear(Vector2 position, float threshold = 0.1f)
    {
        return Vector2.Distance(transform.position, position) <= threshold;
    }
}
