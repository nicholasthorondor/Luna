using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] int health = 1;
    [SerializeField] int attackPower = 1;
    [SerializeField] float movementSpeed = 1f;
    [SerializeField] float aggroRaySize = 2.0f;
    [SerializeField] float knockbackPower = 5f;
    [SerializeField] bool patrollingEnemy = true;

    Rigidbody2D rigidBody;
    bool aggroed;
    int startHealth;
    bool patrollingLeft;
    bool facingRight;
    SpriteRenderer spriteRenderer;
    Animator anim;
    float timeSinceLastAggro = 0;
    float timeSinceDirectionFaceChange = 0;
    Vector3 startingPosition; // The starting position of the enemy.
    bool startsAsAPatrollingEnemy; // The status of whether an enemy patrols or not at the start of a level.

    public int Health {
        get {
            return health;
        } set {
            health = value;
        }
    }

    void Start () {
        startHealth = health;
        rigidBody = GetComponent<Rigidbody2D> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
        anim = GetComponent<Animator> ();
        startingPosition = transform.position;
        startsAsAPatrollingEnemy = patrollingEnemy;
        // Sets the facing right boolean according to flip x value or localscale value.
        if (spriteRenderer.flipX || transform.localScale.x < 0) {
            facingRight = false;
        } else {
            facingRight = true;
        }
        // Randomly sets enemies initial patrol path.
        if (Random.Range (0, 2) == 1) {
            patrollingLeft = true;
        } else {
            patrollingLeft = false;
        }
    }

    void Update () {
        IsAggro ();
        IsEnemyDead ();
        FollowPlayer ();
        RandomLookDirection ();
        if (!aggroed) {
            timeSinceLastAggro += Time.deltaTime; // Tracks the time since last aggro.
        }
        // Checks if the level has been completed. If it has freeze all enemies, preventing them attacking the player.
        if (GameManager.instance.LevelComplete) {
            rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            patrollingEnemy = false;
            anim.SetBool ("Running", false);
            GetComponent<CapsuleCollider2D> ().enabled = false;
        }
    }

    void FixedUpdate () {
        Patrol ();
    }

    void IsAggro () {
        Vector2 position = transform.position; // The current position of the enemy.
        Vector2 direction;
        if (facingRight) {
            direction = Vector2.right; // Direction that the ray shoots.
        } else {
            direction = Vector2.left; // Direction that the ray shoots.
        }
        float rayDistance = aggroRaySize; // The length of the ray shooting from the enemy outwards.

        // Uncomment below to show visual representation of the ray.
        Debug.DrawRay (position, direction * rayDistance, Color.red);

        // Sends out a ray from the enemy position looking for layers that are classified as player.
        RaycastHit2D hit = Physics2D.Raycast (position, direction, rayDistance, GameManager.instance.WhatIsPlayer);

        // If the ray hits something that is the player set the aggroed boolean.
        if (hit.collider != null && timeSinceLastAggro > 1) {
            aggroed = true;
            SoundManagerScript.PlaySound ("aggro");
            timeSinceLastAggro = 0;
        }
    }

    void FollowPlayer () {
        if (aggroed) {
            // If the enemy is not a ptrolling enemy, make the enemy patrol and subsequently follow the player if they are aggroed.
            if (!patrollingEnemy) {
                patrollingEnemy = true;
            }
            Vector3 playerPos = GameManager.instance.Player.transform.position;
            // Follow player movements if aggroed.
            if (playerPos.x > transform.position.x + 0.5f && Mathf.Abs(playerPos.y - transform.position.y) < 0.5f) {
                patrollingLeft = false;
            } else if (playerPos.x < transform.position.x - 0.5f && Mathf.Abs(playerPos.y - transform.position.y) < 0.5f) {
                patrollingLeft = true;
            }
        }
    }

    void Patrol () {
        // Patrol between gameobject patrol points.
        if (patrollingEnemy) {
            anim.SetBool ("Running", true);
            if (patrollingLeft) {
                rigidBody.velocity = new Vector2 (-movementSpeed, rigidBody.velocity.y);
            } else {
                rigidBody.velocity = new Vector2 (movementSpeed, rigidBody.velocity.y);
            }
            if (patrollingLeft && facingRight) {
                Flip ();
            } else if (!patrollingLeft && !facingRight) {
                Flip ();
            }
        }
        // If the enemy started as a non patrolling enemy, return to their starting location once no longer aggroed.
        if (!aggroed && Mathf.Abs (transform.position.x) - Mathf.Abs (startingPosition.x) > 0 && !startsAsAPatrollingEnemy) {
            anim.SetBool ("Running", false);
            patrollingEnemy = false;
        }
    }

    void IsEnemyDead () {
        if (health <= 0) {
            this.gameObject.SetActive (false);
            health = startHealth;
        }
    }

    // Method that simply reverses the direction the enemy sprite is facing in the x plane.
    void Flip () {
        facingRight = !facingRight; // Reverses the facing right boolean.
        spriteRenderer.flipX = !spriteRenderer.flipX; // Reverses the sprite flip x status.
    }

    // Periodically randomises which direction a non patrolling enemy faces.
    void RandomLookDirection () {
        if (!patrollingEnemy) {
            timeSinceDirectionFaceChange += Time.deltaTime;
            if (timeSinceDirectionFaceChange > 4) {
                if (Random.Range (0, 2) == 1) {
                    Flip ();
                    timeSinceDirectionFaceChange = 0;
                } else {
                    timeSinceDirectionFaceChange = 0;
                }
            }
        }
    }

    void OnCollisionEnter2D (Collision2D collision) {
        if (collision.collider.tag == "Player" && health > 0) {
            // Trigger attack animation.
            anim.SetTrigger ("Attack");
            GameManager.instance.Player.GetComponent<PlayerController> ().Anim.SetTrigger ("Knockback"); // Trigger knockback animation on player.
            GameManager.instance.Player.GetComponent<PlayerController> ().enabled = false; // Disables the player controller to allow for a little knockback.
            GameManager.instance.Player.GetComponent<Rigidbody2D> ().AddForce (Vector2.left * knockbackPower, ForceMode2D.Impulse); // Applies knockback.
            SoundManagerScript.PlaySound ("enemyAttack");
            Invoke ("EnablePlayerController", 0.5f); // Reenables player controller after 0.5 seconds.
            // Reduce health of player.
            GameManager.instance.Player.GetComponent<PlayerController> ().Health -= attackPower;
        }
    }

    void OnTriggerEnter2D (Collider2D collision) {
        if (collision.tag == "Patrol Point") {
            // Reverses the direction that the enemy is patrolling.
            patrollingLeft = !patrollingLeft;
            // If a patrol point is hit the enemy will lose aggro status and stop pursuing the player.
            aggroed = false;
        }
        // If the enemy some how falls into the deathwall reset their position and disable them.
        if (collision.tag == "DeathWall") {
            transform.position = startingPosition;
            this.gameObject.SetActive (false);
        }
    }

    void EnablePlayerController () {
        GameManager.instance.Player.GetComponent<PlayerController> ().enabled = true;
    }
}
