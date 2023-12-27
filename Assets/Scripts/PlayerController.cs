using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement
    private float groundDrag = 2;
    public float airMultiplier;
    private float walkSpeed = 7;
    private float sprintSpeed = 13;
    private float horizontalInput;
    private float verticalInput;
    public bool isMoveable;
    Vector3 moveDirection;
    private bool isSprinting;

    // Keybinds
    private KeyCode sprintKey = KeyCode.LeftShift;
    private KeyCode shootKey = KeyCode.Mouse0;
    private KeyCode passKey = KeyCode.Mouse1;

    public float playerHeight;
    public LayerMask whatIsGround;
    private bool grounded;

    // Ball
    public bool hasBall;
    public bool canReceiveBall;

    // Shooting
    private float shotPower = 65;
    private float shotAngle = 0.23f;

    // Passing
    private float passPower = 52;

    // General
    public Transform orientation;
    private Rigidbody rb;
    private Animator playerAnim;
    private GameObject playerModel;
    private GameManager gameManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerAnim = GetComponent<Animator>();
        hasBall = false;
        canReceiveBall = true;
        isSprinting = false;
        isMoveable = true;
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        playerModel = GameObject.Find("Model");
    }

    private void Update()
    {
        // input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isSprinting = Input.GetKey(sprintKey);

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

       SpeedControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        // Sprint, walk, and idle animations
        if((horizontalInput != 0 || verticalInput != 0) && isSprinting)
        {
            playerAnim.SetFloat("Speed", 2);
        } else if(horizontalInput != 0 || verticalInput != 0)
        {
            playerAnim.SetFloat("Speed", 1);
        } else if(horizontalInput == 0 && verticalInput == 0)
        {
            playerAnim.SetFloat("Speed", 0);
        }

        // Stick ball to player if hasBall is ture
        if(hasBall)
        {
            Vector3 ballPositionOffset = playerModel.transform.forward * 1.2f;
            Vector3 ballPosition = new Vector3(playerModel.transform.position.x + ballPositionOffset.x, 0.93f, playerModel.transform.position.z + ballPositionOffset.z);
            gameManager.currentBall.transform.position = ballPosition;
        }

        // Shooting and passing
        if(Input.GetKeyDown(shootKey))
        {
            StartCoroutine(Shoot());
        } else if(Input.GetKeyDown(passKey))
        {
            StartCoroutine(Pass());
        } else
        {
            playerAnim.SetFloat("Ball Power", 0);
        }
    }

    private void FixedUpdate()
    {
        if (hasBall)
        {
            gameManager.currentBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(isMoveable)
        {
            // on ground
            if (grounded)
                rb.AddForce(moveDirection.normalized * (isSprinting ? sprintSpeed : walkSpeed) * 10f * rb.mass, ForceMode.Force);

            // in air
            else if (!grounded)
                rb.AddForce(moveDirection.normalized * (isSprinting ? sprintSpeed : walkSpeed) * 10f * airMultiplier * rb.mass, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (isSprinting && flatVel.magnitude > sprintSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * sprintSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        } else if(!isSprinting && flatVel.magnitude > walkSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * walkSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.Equals(gameManager.currentBall) && canReceiveBall)
        {
            hasBall = true;
            gameManager.opposingPlayerRef.GetComponent<OpposingPlayer>().hasBall = false;
        }
    }

    IEnumerator Shoot()
    {
        playerAnim.SetFloat("Ball Power", 2);
        if (hasBall)
        {
            canReceiveBall = false;
            yield return new WaitForSeconds(0.3f);
            hasBall = false;
            gameManager.currentBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Vector3 shotVector = new Vector3(gameManager.currentBall.transform.position.x - transform.position.x, shotAngle, gameManager.currentBall.transform.position.z - transform.position.z);
            gameManager.currentBall.GetComponent<Rigidbody>().AddForce(shotVector * shotPower, ForceMode.Impulse);
            Invoke("setCanReceiveBallTrue", 1f);
        }
    }

    IEnumerator Pass()
    {
        playerAnim.SetFloat("Ball Power", 1);
        if (hasBall)
        {
            canReceiveBall = false;
            yield return new WaitForSeconds(0.3f);
            hasBall = false;
            gameManager.currentBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Vector3 passVector = new Vector3(gameManager.currentBall.transform.position.x - transform.position.x, 0, gameManager.currentBall.transform.position.z - transform.position.z);
            gameManager.currentBall.GetComponent<Rigidbody>().AddForce(passVector * passPower, ForceMode.Impulse);
            Invoke("setCanReceiveBallTrue", 1f);
        }
    }

    private void setCanReceiveBallTrue()
    {
        canReceiveBall = true;
    }
}
