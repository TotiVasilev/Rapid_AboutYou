using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float raycastDistance = 1f; // Distance to check for obstacles
    public float avoidanceDistance = 1f; // How far to try avoiding obstacles
    public LayerMask obstacleLayer; // Layer mask to identify obstacles
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Sprite upSprite, downSprite, leftSprite, rightSprite;
    public Sprite upSpriteWithBucket, downSpriteWithBucket, leftSpriteWithBucket, rightSpriteWithBucket;

    private Vector2 targetPosition;
    private bool isHoldingBucket = false;
    private bool isMoving = false;

    // Track the player's facing direction
    private enum Direction { Up, Down, Left, Right, None }
    private Direction lastDirection = Direction.None;

    void Start()
    {
        // Ensure the Rigidbody2D is set to Kinematic to prevent falling
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Set the initial position and sprite
        targetPosition = transform.position;
        lastDirection = Direction.Right;
        UpdateSprite();
    }

    void Update()
    {
        // Check for left mouse click and update target position
        if (Input.GetMouseButtonDown(0))  // Left click (0)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;  // Ensure the target position is on the same plane as the player
            targetPosition = new Vector2(mousePos.x, mousePos.y);
            isMoving = true;  // Player is now moving towards the target position
        }

        // Update sprite based on the direction of movement
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
        // Get the direction to the target
        Vector2 direction = targetPosition - (Vector2)transform.position;

        // Raycast in the direction of movement to check for obstacles
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, raycastDistance, obstacleLayer);

        if (hit.collider != null) // If there is an obstacle in the way
        {
            // Attempt to avoid the obstacle by checking left and right
            Vector2 avoidanceDirection = TryAvoidObstacle(direction);
            rb.linearVelocity = avoidanceDirection * moveSpeed;  // Apply velocity to move around the obstacle
        }
        else // No obstacle, move towards the target normally
        {
            if (direction.magnitude > 0.1f)  // Continue moving until the player is close to the target
            {
                rb.linearVelocity = direction.normalized * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;  // Stop moving once the target is reached
                isMoving = false;  // Stop the movement
            }
        }
    }

    // Try to avoid the obstacle by checking left and right for a clear path
    private Vector2 TryAvoidObstacle(Vector2 direction)
    {
        // Check for obstacles to the left and right of the player
        Vector2 leftDirection = Quaternion.Euler(0, 0, 90) * direction; // Rotate direction 90 degrees counter-clockwise
        Vector2 rightDirection = Quaternion.Euler(0, 0, -90) * direction; // Rotate direction 90 degrees clockwise

        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, leftDirection, avoidanceDistance, obstacleLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, rightDirection, avoidanceDistance, obstacleLayer);

        if (hitLeft.collider == null) // No obstacle to the left, move left
        {
            return leftDirection;
        }
        else if (hitRight.collider == null) // No obstacle to the right, move right
        {
            return rightDirection;
        }
        else // If both left and right are blocked, keep moving forward
        {
            return direction.normalized;
        }
    }

    // Update the last facing direction based on the movement direction
    private void UpdateFacingDirection()
    {
        if (isMoving)
        {
            Vector2 direction = targetPosition - (Vector2)transform.position;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                lastDirection = direction.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                lastDirection = direction.y > 0 ? Direction.Up : Direction.Down;
            }
        }
    }

    // Function to update the sprite based on the facing direction
    private void UpdateSprite()
    {
        if (isHoldingBucket)
        {
            // Player is holding the bucket, update sprite accordingly
            switch (lastDirection)
            {
                case Direction.Right:
                    spriteRenderer.sprite = rightSpriteWithBucket;
                    break;
                case Direction.Left:
                    spriteRenderer.sprite = leftSpriteWithBucket;
                    break;
                case Direction.Up:
                    spriteRenderer.sprite = upSpriteWithBucket;
                    break;
                case Direction.Down:
                    spriteRenderer.sprite = downSpriteWithBucket;
                    break;
                default:
                    spriteRenderer.sprite = rightSpriteWithBucket; // Default to right if facing direction is None
                    break;
            }
        }
        else
        {
            // Player is not holding the bucket, update sprite to normal
            switch (lastDirection)
            {
                case Direction.Right:
                    spriteRenderer.sprite = rightSprite;
                    break;
                case Direction.Left:
                    spriteRenderer.sprite = leftSprite;
                    break;
                case Direction.Up:
                    spriteRenderer.sprite = upSprite;
                    break;
                case Direction.Down:
                    spriteRenderer.sprite = downSprite;
                    break;
                default:
                    spriteRenderer.sprite = rightSprite; // Default to right if facing direction is None
                    break;
            }
        }
    }

    // Function to pick up the bucket
    public void PickUpBucket()
    {
        isHoldingBucket = true;
        UpdateSprite();  // Update sprite immediately when the bucket is picked up
    }

    // Function to drop the bucket
    public void DropBucket()
    {
        isHoldingBucket = false;
        UpdateSprite();  // Update sprite immediately when the bucket is dropped
    }
}
