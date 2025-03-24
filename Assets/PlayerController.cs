using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Sprite upSprite, downSprite, leftSprite, rightSprite;
    public Sprite upSpriteWithBucket, downSpriteWithBucket, leftSpriteWithBucket, rightSpriteWithBucket;

    private Vector2 movement;
    private bool isHoldingBucket = false;

    // Track the player's facing direction
    private enum Direction { Up, Down, Left, Right, None }
    private Direction lastDirection = Direction.None;

    void Start()
    {
        // Ensure the Rigidbody2D is set to Kinematic to prevent falling
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Initialize the player's facing direction (set to a default, like "Right" or "Down")
        lastDirection = Direction.Right; // or you can choose Direction.Left, Direction.Up, etc. based on your preference

        // Update sprite immediately based on the initial facing direction
        UpdateSprite();
    }

    void Update()
    {
        // Get input from player
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Prevent diagonal movement from being faster
        if (movement.x != 0)
            movement.y = 0;

        // Update the last direction the player is facing
        UpdateFacingDirection();

        // Update sprite based on the last facing direction
        UpdateSprite();
    }

    void FixedUpdate()
    {
        // Move the player
        rb.linearVelocity = movement * moveSpeed;
    }

    // Update the last facing direction based on input
    private void UpdateFacingDirection()
    {
        if (movement.y < 0) lastDirection = Direction.Down;
        else if (movement.x < 0) lastDirection = Direction.Left;
        else if (movement.y > 0) lastDirection = Direction.Up;
        else if (movement.x > 0) lastDirection = Direction.Right;
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
