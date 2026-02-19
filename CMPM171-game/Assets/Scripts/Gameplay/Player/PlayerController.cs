using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;                                                         // Movement speed of the player
    public float rollSpeed = 15f;                                                        // Speed during roll
    public float rollDuration = 0.3f;                                                    // Duration of the roll
    public float rollCooldown = 0.5f;                                                    // Cooldown between rolls

    private Rigidbody2D rb;  
    private Animator anim;                                                              // Rigidbody2D component of the player
    private Vector2 move;                                                                // Movement direction vector
    private Player_settings playerSettings;                                              // Reference to Player_settings component
    private bool isRolling = false;                                                      // Whether the player is currently rolling
    private float rollEndTime = 0f;                                                      // Time when the current roll ends
    private float lastRollTime = -999f;                                                  // Time when the last roll was performed
    private Vector2 rollDirection;                                                       // Direction of the current roll
    private Vector2 lastMoveDirection = Vector2.right;                                   // Last movement direction (default to right)

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

        // Check for roll input (Space key)
        if (Input.GetKeyDown(KeyCode.Space) && !isRolling && Time.time >= lastRollTime + rollCooldown)
        {
            // Determine roll direction based on current movement input, or use last movement direction
            if (move.magnitude > 0.1f)
            {
                rollDirection = move.normalized;
            }
            else
            {
                // If no movement input, roll in the last movement direction
                rollDirection = lastMoveDirection;
            }

            // Start roll
            isRolling = true;
            rollEndTime = Time.time + rollDuration;
            lastRollTime = Time.time;

            // Set invincible state
            if (playerSettings != null)
            {
                playerSettings.SetInvincible(true);
            }
        }

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
}
