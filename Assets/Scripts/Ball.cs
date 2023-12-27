using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(gameObject.Equals(gameManager.currentBall) && gameManager.canScoreGoal)
        {
            if (other.CompareTag("Opposing Goal"))
            {
                StartCoroutine(gameManager.GoalScored(true));
            } else
            {
                StartCoroutine(gameManager.GoalScored(false));
            }     
        }
    }

    
}
