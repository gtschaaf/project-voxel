using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed;
    public float jumpForce;
    public bool onGround;

    private Rigidbody2D rb;
    private Animator animator;

    public float horizontal;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    //Trigger if player standing on solid ground
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Ground"))
        {
            onGround = true;
        }
    }

    //Trigger if player not on solid ground
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Ground"))
        {
            onGround = false;
        }

    }

    private void FixedUpdate()
    {
        //Set A and D keys to move horizontally
        horizontal = Input.GetAxis("Horizontal");
        //Set jump key to spacebar
        float jump = Input.GetAxisRaw("Jump");
        //Set up arrow key to also jump 
        //GetAxisRaw allows jump to be more responsive
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal * movementSpeed, rb.velocity.y);

        if (horizontal > 0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(horizontal < 0) {
            transform.localScale = new Vector3(1, 1, 1);
        }

      
        if ( jump > 0.1f || vertical > 0.1f)
        {
            //Make sure player is on ground before jumping
            if (onGround)
            {
                movement.y = jumpForce;
            }

           


        }
        //Rb.velocity.y allows character to fall, a value of 0 would cause them to float
        rb.velocity = movement;
    }

    private void Update()
    {
        animator.SetFloat("Horizontal", horizontal);
    }

}