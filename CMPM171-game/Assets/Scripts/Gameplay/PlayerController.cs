using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;                                                         // Movement speed of the player

    private Rigidbody2D rb;                                                              // Rigidbody2D component of the player
    private Vector2 move;                                                                // Movement direction vector

    // Awake method to initialize the Rigidbody2D component
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update method to get input and calculate movement direction
    void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    // FixedUpdate method to apply movement to the player
    void FixedUpdate()
    {
        rb.linearVelocity = move * moveSpeed;
    }
}
