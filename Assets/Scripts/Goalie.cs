using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goalie : MonoBehaviour
{
    private Animator playerAnim;
    GameManager gameManager;
    private const float NET_BOUND = 5f;
    public float speed = 5;
    public bool canReceiveBall = true;

    void Start()
    {
        playerAnim = GetComponent<Animator>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
       if(Mathf.Abs(gameManager.currentBall.transform.position.z) < NET_BOUND)
        {
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, gameManager.currentBall.transform.position.z), step);
            playerAnim.SetBool("isShuffling", true);
        }
        else
        {
            playerAnim.SetBool("isShuffling", false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.Equals(gameManager.currentBall) && canReceiveBall)
        {
            playerAnim.SetTrigger("grabBall");
            StartCoroutine(gameManager.GoaliePossess());
        }
    }
}
