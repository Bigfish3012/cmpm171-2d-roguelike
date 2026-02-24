using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Bounds")]
    [Tooltip("Optional. If set, player movement will be clamped to this object's bounds (e.g. Cutting_board).")]
    [SerializeField] private Transform movementBounds;
    [Tooltip("Inset from each edge of the bounds so player does not clip.")]
    [SerializeField] private float boundsPadding = 0.5f;

    public float moveSpeed = 6f;                                                         // Movement speed of the player
    public float rollSpeed = 15f;                                                        // Speed during roll
    public float rollDuration = 0.3f;                                                    // Duration of the roll
    public float rollCooldown = 0.5f;                                                    // Cooldown between rolls
    public float doubleTapWindow = 0.3f;                                                 // Max time between two taps to count as double-tap

    private Rigidbody2D rb;  
    private Animator anim;                                                              // Rigidbody2D component of the player
    private Vector2 move;                                                                // Movement direction vector
    private Player_settings playerSettings;                                              // Reference to Player_settings component
    private bool isRolling = false;                                                      // Whether the player is currently rolling
    private float rollEndTime = 0f;                                                      // Time when the current roll ends
    private float lastRollTime = -999f;                                                  // Time when the last roll was performed
    private Vector2 rollDirection;                                                       // Direction of the current roll
    private Vector2 lastMoveDirection = Vector2.right;                                   // Last movement direction (default to right)
    private float lastUpKeyTime = -999f;                                                 // Last time Up key was pressed
    private float lastDownKeyTime = -999f;                                               // Last time Down key was pressed
    private float lastLeftKeyTime = -999f;                                               // Last time Left key was pressed
    private float lastRightKeyTime = -999f;                                              // Last time Right key was pressed

    // Awake method to initialize the Rigidbody2D component
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerSettings = GetComponent<Player_settings>();
    }

    // Update method to get input and calculate movement direction
    void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        
        handleAnimations();

        // Update last movement direction if player is moving
        if (move.magnitude > 0.1f)
        {
            lastMoveDirection = move.normalized;
        }

        // Check for roll input (double-tap direction key)
        TryRollOnDoubleTap(KeyCode.W, KeyCode.UpArrow, Vector2.up, ref lastUpKeyTime);
        TryRollOnDoubleTap(KeyCode.S, KeyCode.DownArrow, Vector2.down, ref lastDownKeyTime);
        TryRollOnDoubleTap(KeyCode.A, KeyCode.LeftArrow, Vector2.left, ref lastLeftKeyTime);
        TryRollOnDoubleTap(KeyCode.D, KeyCode.RightArrow, Vector2.right, ref lastRightKeyTime);

        // Check if roll should end
        if (isRolling && Time.time >= rollEndTime)
        {
            isRolling = false;
            // Remove invincible state
            if (playerSettings != null)
            {
                playerSettings.SetInvincible(false);
            }
        }
    }

    // Check if direction key was double-tapped and start roll in that direction
    void TryRollOnDoubleTap(KeyCode key1, KeyCode key2, Vector2 direction, ref float lastKeyTime)
    {
        if (!Input.GetKeyDown(key1) && !Input.GetKeyDown(key2))
            return;

        if (Time.time - lastKeyTime < doubleTapWindow && !isRolling && Time.time >= lastRollTime + rollCooldown)
        {
            rollDirection = direction;
            isRolling = true;
            rollEndTime = Time.time + rollDuration;
            lastRollTime = Time.time;

            if (playerSettings != null)
            {
                playerSettings.SetInvincible(true);
            }
        }

        lastKeyTime = Time.time;
    }

    void handleAnimations()
    {
        bool isMove = move.magnitude > 0.1f;
        anim.SetBool("isMove", isMove);
    }

    // FixedUpdate method to apply movement to the player
    void FixedUpdate()
    {
        if (isRolling)
        {
            // Apply roll movement
            rb.linearVelocity = rollDirection * rollSpeed;
        }
        else
        {
            // Normal movement
            rb.linearVelocity = move * moveSpeed;
        }
    }

    // Upgrade method for Level Up Menu
    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }

    // Clamp player position to movement bounds (e.g. cutting board) after physics
    void LateUpdate()
    {
        if (movementBounds == null) return;

        Bounds bounds;
        var sr = movementBounds.GetComponent<SpriteRenderer>();
        var col = movementBounds.GetComponent<Collider2D>();
        if (sr != null)
            bounds = sr.bounds;
        else if (col != null)
            bounds = col.bounds;
        else
            return;

        float minX = bounds.min.x + boundsPadding;
        float maxX = bounds.max.x - boundsPadding;
        float minY = bounds.min.y + boundsPadding;
        float maxY = bounds.max.y - boundsPadding;

        Vector2 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        rb.position = pos;
    }
}
