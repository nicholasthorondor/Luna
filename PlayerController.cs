using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float movementSpeed = 10; // The speed at which the player moves at. Set in the inspector.
    [SerializeField] float jumpPower = 10; // The power applied to the players jump. ie. how high the player jumps. Set in the inspector.
    [SerializeField] float groundCheckRaySize = 1.1f; // The size of the ray shooting towards the groound from the player.
    [SerializeField] float globalGravityScale = -30f; // Allows adjustment of the gravity scale to fine tune movement feel.
    [SerializeField] PhysicsMaterial2D groundedMaterial = null; // The physics material applied when the player is on the ground.
    [SerializeField] PhysicsMaterial2D airborneMaterial = null; // The physics material applied when the player is in the air.
    [SerializeField] int health = 1; // The max health of the player.
    [SerializeField] int attackPower = 1; // The attack power of the player.
    [SerializeField] float bounceRebound = 15f; // the amount of bounce applied when jumping on an enemy to kill them.

    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;
    Animator anim;
    bool facingRight; // Used to track whether the player is facing right or not.
    bool grounded; // Used to track whther the player is on the ground or not.
    bool beenInAir; // Used to track whether the player has landed or not.
    int startHealth; // The starting health of the player.

    public bool Grounded
    {
        get
        {
            return grounded;
        }
    }

    public int Health {
        get {
            return health;
        }
        set {
            health = value;
        }
    }

    public Animator Anim {
        get {
            return anim;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.Player = this.gameObject;
        Physics2D.gravity = new Vector3(0, globalGravityScale, 0); // Sets the global gravity scale of the game.
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        startHealth = health;

        // Sets the facing right boolean according to flip x value or localscale value.
        if (spriteRenderer.flipX || transform.localScale.x < 0)
        {
            facingRight = false;
        }
        else
        {
            facingRight = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        AnimationStateToggle();
        Jump();
        IsPlayerDead ();
    }

    // Use for most rigidbody changes with physics applied over time.
    void FixedUpdate()
    {
        IsGrounded();
        Move();
        LandSFX();
    }

    // Method controlling player movement.
    void Move()
    {
        float horizontalMovement = Input.GetAxis("Horizontal"); // Gets the horizontal input value.
        rigidBody.velocity = new Vector2(horizontalMovement * movementSpeed, rigidBody.velocity.y); // Applies velocity in the x plane to the player.
        anim.SetFloat("hSpeed", Mathf.Abs(horizontalMovement));
        // Flips the player's sprite depending on movement direction.
        if (horizontalMovement > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalMovement < 0 && facingRight)
        {
            Flip();
        }
    }

    void LandSFX()
    {
        // If the player is not grounded set boolean.
        if (!grounded)
        {
            beenInAir = true;
        }
        // If the player is grounded and has been in the air.
        if (grounded && beenInAir)
        {
            // Prevents sound effect from triggering at the very start of the game.
            if (Time.timeSinceLevelLoad < 1f)
            {
                beenInAir = false;
                return;
            }
            beenInAir = false;
            SoundManagerScript.PlaySound("land");
        }
    }

    // Method that simply reverses the direction the player sprite is facing in the x plane.
    void Flip()
    {
        facingRight = !facingRight; // Reverses the facing right boolean.
        spriteRenderer.flipX = !spriteRenderer.flipX; // Reverses the sprite flip x status.
    }

    // Method that controls the player's jumping.
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && grounded)
        {
            SoundManagerScript.PlaySound("jump");
            rigidBody.velocity = Vector2.zero; // Resets any preliminary force applid to the player to prevent super jumps.
            rigidBody.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse); // Adds upwards froce to the player.
            anim.SetBool("Jumping", true); // Sets the jumping animation state.
        }
    }

    // Method to check if the player is on the ground or not.
    void IsGrounded()
    {
        Vector2 position = transform.position; // The current position of the player.
        Vector2 direction = Vector2.down; // Direction that the ray shoots.
        float rayDistance = groundCheckRaySize; // The length of the ray shooting from the player toward the ground.

        // Uncomment below to show visual representation of the ray.
        //Debug.DrawRay (position, direction * rayDistance, Color.red);

        // Sends out a ray from the player position looking for layers that are classified as ground.
        RaycastHit2D hit = Physics2D.Raycast(position, direction, rayDistance, GameManager.instance.WhatIsGround);

        // If the ray hits something that is ground set the grounded boolean.
        if (hit.collider != null)
        {
            grounded = true;
            rigidBody.sharedMaterial = groundedMaterial; // Applies the grounded physics material.
            //anim.SetBool ("Jumping", false); // Sets the jumping animation state.
        }
        else
        {
            grounded = false;
            rigidBody.sharedMaterial = airborneMaterial; // Applies the airborne physics material.
        }
    }

    // Method to toggle animation states based upon certain conditions.
    void AnimationStateToggle()
    {
        // If the player is grounded and velocity in the y plane is minimal set falling and jumping animation states to false.
        if (grounded && rigidBody.velocity.y <= 0.1 && rigidBody.velocity.y > -1)
        {
            anim.SetBool("Jumping", false);
            anim.SetBool("Falling", false);
        }

        // If the player is in the air and velocity in the y plane is negative past a threshold set falling animation state to true.
        if (!grounded && rigidBody.velocity.y < -1)
        {
            anim.SetBool("Falling", true);
        }
    }

    void IsPlayerDead () {
        if (health <= 0) {
            GameManager.instance.ResetLevel ();
            health = startHealth;
        }
    }

    // Method for handling triggers.
    void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player falls into the death wall trigger, reset their location to the respawn point.
        if (collision.tag == "DeathWall")
        {
            GameManager.instance.ResetLevel ();
        }

        // If the player collects a sun piece.
        if (collision.tag == "Sun Piece")
        {
            SoundManagerScript.PlaySound("collect_item");
            collision.gameObject.SetActive(false); // Disables the sun piece from the level.
            GameManager.instance.SunPiecesCollected++; // Increment the number of sun pieces collected.
            float currentLightIntensity = GameManager.instance.DirectionalLight.intensity; // Gets the light intensity at the point of sun piece collection.
            GameManager.instance.CurrentLightIntensity = currentLightIntensity; // Sets the current light intensity in the GameManager.
            GameManager.instance.LerpCounter = 0;
            GameManager.instance.IncreaseLight = true; // Set the boolean in the gamemanger to trigger brightening the scene.
            // Get the UI sun piece colected image and change the sprite to represent the number of collected sun pieces.
            GameManager.instance.SunPieceUIImage.GetComponent<Image>().sprite = GameManager.instance.SunPieceSprites[GameManager.instance.SunPiecesCollected];
        }

        // If the player jumps on an enemies head.
        if (collision.tag == "Enemy") {
            // Damage the enemy.
            if (collision.GetComponent<EnemyController> ()) {
                SoundManagerScript.PlaySound ("jumpAttack");
                collision.GetComponent<EnemyController> ().Health -= attackPower;
                rigidBody.velocity = Vector2.zero;
                rigidBody.AddForce (Vector2.up * bounceRebound, ForceMode2D.Impulse);
            }
        }
    }
}
