using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpposingPlayer : MonoBehaviour
{
    private Rigidbody rb;
    private Animator playerAnim;
    private GameManager gameManager;

    public bool hasBall;
    public bool canReceiveBall;
    public bool canMove;

    public float runSpeed = 5;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        hasBall = false;
        canReceiveBall = true;
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        SpeedControl();
        
        if(hasBall)
        {
            gameManager.currentBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Vector3 ballPositionOffset = transform.forward * 1.4f;
            Vector3 ballPosition = new Vector3(transform.position.x + ballPositionOffset.x, 0.93f, transform.position.z + ballPositionOffset.z);
            gameManager.currentBall.transform.position = ballPosition;
            transform.LookAt(GameObject.Find("Player Goal").transform.position);
        } else
        {
            transform.LookAt(gameManager.currentBall.transform);
        }

        // Run animation
        if (rb.velocity != Vector3.zero)
        {
            playerAnim.SetBool("isRunning", true);
        }
        else
        {
            playerAnim.SetBool("isRunning", true);
        }
    }

    private void FixedUpdate()
    {
        if(canMove)
        {
            if (hasBall)
            {
                rb.AddForce((GameObject.Find("Player Goal").transform.position - transform.position) * runSpeed * rb.mass);
            }
            else
            {
                if (gameManager.currentBall.transform.position.y < 4)
                {
                    rb.AddForce((gameManager.currentBall.transform.position - transform.position) * runSpeed * rb.mass);
                }
            }
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > runSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * runSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.Equals(gameManager.currentBall) && canReceiveBall)
        {
            hasBall = true;
            gameManager.playerRef.GetComponent<PlayerController>().hasBall = false;
        }
    }
}
