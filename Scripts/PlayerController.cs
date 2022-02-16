using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player parameters tweaked in design
    public float hInput;
    public float vInput;
    public float moveSpeed = 15f;
    public float climbSpeed = 15f;
    public float gDistance = 0.5f;
    public float gValue = -9.81f;
    public float wDistance;
    public float jumpHeight;    
    public float wallJumpHeight;

    // Player parameters
    public Vector3 gForce;

    public Rigidbody rb;
    public LayerMask ground;

    // State Machine
    enum currentState {Idle, Running,Falling,Jumping, Climbing, Launch, Emotion1, EJump, Emotion2};
    currentState state = currentState.Idle;

    // Vectors
    Vector2 hvec;
    Vector2 vvec;

    Vector3 jVec;
    Vector3 wJVec;

    // Variables used in logic and input
    float jInput;
    bool isGrounded;
    bool isLeftWall;
    bool isRightWall;
    int isOnWall;
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("State is" + state);
        Debug.Log(OnWall() + " 1 is left, 2 is right");
        Debug.Log("Grounded:" + groundCheck());
        Debug.Log("X velocity is" + rb.velocity.x);
        if (state == currentState.Idle)
        {
            move();
            jump();
            gravity();
            if (rb.velocity.y > 0)
            {
                state = currentState.Jumping;
            }
            if (rb.velocity.y < 0)
            {
                state = currentState.Falling;
            }
            if (OnWall() == 1 || OnWall() == 2)
            {
                state = currentState.Climbing;
            }
            if (rb.velocity.x != 0 && groundCheck())
            {
                state = currentState.Running;
            }
            
        }
        if (state == currentState.Running)
        {
            move();
            jump();
            gravity();
            if(rb.velocity.y > 0)
            {
                state = currentState.Jumping;
            }
            if (rb.velocity.y < 0)
            {
                state = currentState.Falling;
            }
            if (OnWall() == 1 || OnWall() == 2)
            {
                state = currentState.Climbing;
            }
            if (rb.velocity.x == 0 && groundCheck())
            {
                state = currentState.Idle;
            }
        }
        if (state == currentState.Falling)
        {
            move();
            gravity();
            if (groundCheck() && rb.velocity.x == 0)
            {
                state = currentState.Idle;
            }
            if (groundCheck() && rb.velocity.x != 0)
            {
                state = currentState.Running;
            }
            if (rb.velocity.y > 0)
            {
                state = currentState.Jumping;
            }
            if (OnWall() == 1 || OnWall() == 2)
            {
                state = currentState.Climbing;
            }
        }
        if (state == currentState.Climbing)
        {
            wallMove();
            wallJump();
            wallFall();
            if (OnWall() == 0)
            {
                if (rb.velocity.y > 0 && OnWall() == 0)
                {
                    state = currentState.Launch;
                }
                
                else if (groundCheck())
                {
                    state = currentState.Idle;
                }
                else if (OnWall() == 0)
                {
                    state = currentState.Falling;
                }
            }
        }
        if (state == currentState.Launch)
        {
            move();
            gravity();
            if (rb.velocity.y<0 && OnWall() == 0)
            {
                state = currentState.Emotion1;
            }
            if (OnWall() == 1 || OnWall() == 2)
            {
                state = currentState.Climbing;
            }
        }
        if (state == currentState.Emotion1)
        {
            move();
            gravity();
            jump();
            if (rb.velocity.y >0 && OnWall() == 0)
            {
                state = currentState.EJump;
            }
            if (rb.velocity.y == 0 && rb.velocity.x == 0)
            {
                state = currentState.Idle;
            }
            if (rb.velocity.y == 0 && rb.velocity.x != 0)
            {
                state = currentState.Running;
            }
        }
        if (state == currentState.EJump)
        {
            move();
            gravity();
            if (rb.velocity.y < 0 && OnWall() == 0)
            {
                state = currentState.Emotion2;
            }
            if (OnWall() == 1 || OnWall() == 2)
            {
                state = currentState.Climbing;
            }

        }
        if (state == currentState.Emotion2)
        {
            move();
            gravity();
            if (rb.velocity.y == 0 && rb.velocity.x == 0)
            {
                state = currentState.Idle;
            }
        }
        if (state == currentState.Jumping)
        {
            move();
            gravity();
            if (OnWall() == 1 || OnWall() == 2)
            {
                state = currentState.Climbing;
            }
            if (groundCheck() && rb.velocity.x == 0)
            {
                state = currentState.Idle;
            }
            if (groundCheck() && rb.velocity.x != 0)
            {
                state = currentState.Running;
            }
            if (rb.velocity.y < 0)
            {
                state = currentState.Falling;
            }
        }
        
    }


    // Function to make player detach from wall
    void wallFall()
    {
        hInput = Input.GetAxis("Horizontal");
        if (OnWall() == 1 && hInput > 0)
        {
            transform.position += new Vector3(0.25f, 0, 0);
            rb.AddForce(new Vector3(50, -50, 0));
        }
        if (OnWall() == 2 && hInput < 0)
        {
            transform.position += new Vector3(-0.25f, 0, 0);
            rb.AddForce(new Vector3(-50, -50, 0));
        }
    }

    // Method to make player launch off of wall
    void wallJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (OnWall() == 1)
            {
                transform.position += new Vector3(0.25f, 0, 0);
                wJVec = new Vector3(wallJumpHeight, wallJumpHeight, 0);
                rb.AddForce(wJVec);
            }
            if (OnWall() == 2)
            {
                transform.position += new Vector3(-0.25f, 0, 0);
                wJVec = new Vector3(-wallJumpHeight, wallJumpHeight, 0);
                rb.AddForce(wJVec);
            }

        }
    }

    // Method to check for ground contact
    bool groundCheck()
    {
        RaycastHit gHit;
        return isGrounded = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out gHit, gDistance, ground);
    }


    // Method to know what direction you're moving in
    int moveDirection()
    {
        if (rb.velocity.x > 0)
        {
            return 2;
        }
        else if (rb.velocity.x < 0)
        {
            return 1;
        }
        else
        {
            return 3;
        }
    }


    // Method to know if you are on the wall and what side of the wall you are at returns 1 if on the left, 2 if on right, 0 if no wall, 3 if both probably will do error catching on that
    int OnWall()
    {
        RaycastHit rHit;
        RaycastHit lHit;
        isRightWall = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out rHit, wDistance, ground);
        isLeftWall = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out lHit, wDistance, ground);

        if (isLeftWall)
        {
            return 1;
        }
        else if (isRightWall)
        {
            return 2;
        }
        else if (!isLeftWall && !isRightWall)
        {
            return 0;
        }
        else
        {
            return 3;
        }

    }

    // Method to make player move
    void move()
    {
        hInput = Input.GetAxis("Horizontal");
        hvec.y = rb.velocity.y;
        hvec.x = hInput * moveSpeed;
        rb.velocity = hvec;
    }

    // Method to make player jump
    void jump()
    {
        if (Input.GetButtonDown("Jump")) { 
            jVec = new Vector3(0,jumpHeight, 0);
            //rb.AddForce(jVec);

            rb.velocity = jVec;
        }
    }
   
    // Method that turns gravity on or off or changes gravity
    void gravity()
    {
        gForce.y = gValue;
        rb.AddForce(gForce);
    }

    // Method that handles moving on the wall
    void wallMove()
    {
        vInput = Input.GetAxis("Vertical");
        vvec.x = rb.velocity.x;
        vvec.y = vInput * climbSpeed;
        rb.velocity = vvec;

    }

}
