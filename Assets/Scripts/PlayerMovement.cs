using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public LayerMask groundLayerMask;

    public float speed = 350f;

    public float jump = 15f;
    public float addedJumpIfHold = 3f;
    public float maxJumpHoldTime = .16f;
    public float coyoteDuration = .07f;

    public float maxFallSpeed = 18f;

    public float groundRaycastOffset = .5f;
    public float groundRaycastDistance = 1.52f;

    public float headRaycastOffset = .5f;
    public float headRaycastDistance = .1f;		

    public bool debugShowRaycast = false;
  
    private BoxCollider2D boxCollider2D;
    private Rigidbody2D rigidBody2D;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float horizontalDirection;
    private float jumpTime = 0f;
    private float GroundCheckTurnOffTime = -1f;

    private int direction = 1;

    private bool buttonJumpDown;
    private bool buttonJumpHold;

    private bool isOnGround = false;
    private bool isJumping = false;
    private bool isHeadHitted = false;

    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        horizontalDirection = speed * Input.GetAxis("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(horizontalDirection));

        if (horizontalDirection * direction < 0)
        {
            FlipCharacter();
        }

        if (Input.GetButtonDown("Jump"))
        {
            buttonJumpDown = true;
        }

        if (Input.GetButton("Jump"))
        {
            buttonJumpHold = true;
        }

        animator.SetBool("isJumping", !isOnGround);
    }

    void FixedUpdate()
    {
        CheckRaycast();

        UpdateMovement();
        UpdateJump();

        // Flush commands
        buttonJumpDown = false;
        buttonJumpHold = false;
    }

    private void CheckRaycast()
    {
        if (GroundCheckTurnOffTime > 0f)
        {
            return;
        }

        Vector2 position = transform.position;
        Vector2 groundOffset = new Vector2(groundRaycastOffset, 0f);

        RaycastHit2D leftHit = Physics2D.Raycast(position - groundOffset, Vector2.down, groundRaycastDistance, groundLayerMask);
        RaycastHit2D rightHit = Physics2D.Raycast(position + groundOffset, Vector2.down, groundRaycastDistance, groundLayerMask);
        isOnGround = leftHit || rightHit;

        Vector2 headOffset = new Vector2(0f, headRaycastOffset);

        RaycastHit2D heightHit = Physics2D.Raycast(position + headOffset, Vector2.up, headRaycastDistance, groundLayerMask);
        isHeadHitted = heightHit;

        if (debugShowRaycast)
        {
            Debug.DrawRay(position - groundOffset, Vector2.down * groundRaycastDistance, leftHit ? Color.red : Color.green);
            Debug.DrawRay(position + groundOffset, Vector2.down * groundRaycastDistance, rightHit ? Color.red : Color.green);

            Debug.DrawRay(position + headOffset, Vector2.up * headRaycastDistance, heightHit ? Color.red : Color.green);
        }
    }

    private void UpdateMovement()
    {
        rigidBody2D.velocity = new Vector2(horizontalDirection * Time.fixedDeltaTime, rigidBody2D.velocity.y);
    }

    private void UpdateJump()
    {
        if (!isJumping)
        {
            if (isOnGround && buttonJumpDown)
            {
                isOnGround = false;
                isJumping = true;

                jumpTime = Time.time + maxJumpHoldTime;

                GroundCheckTurnOffTime = 0.1f;  // quick fix to Raycast detecting immediately during jump 

                rigidBody2D.AddForce(new Vector2(0f, jump), ForceMode2D.Impulse);
            }
        }
        else // isJumping = true
        {
            GroundCheckTurnOffTime -= Time.fixedDeltaTime;

            if (buttonJumpHold && Time.time < jumpTime && !isHeadHitted)
            {
                rigidBody2D.AddForce(new Vector2(0f, addedJumpIfHold), ForceMode2D.Impulse);
                
            }

            if (isOnGround)
            {
                isJumping = false;
                buttonJumpHold = false;
                GroundCheckTurnOffTime = -1f;
            }
        }

        if (rigidBody2D.velocity.y > maxFallSpeed)
        {
            rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, maxFallSpeed);
        }
        else if (rigidBody2D.velocity.y < -maxFallSpeed)
        {
            rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, -maxFallSpeed);
        }

    }

    private void FlipCharacter()
    {
        direction *= -1;

        if (direction > 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }
}
