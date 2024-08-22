using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    GameManager gameManager;
    void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.transform.root.CompareTag("Player"))
        {
            gameManager.GameOver();
        }
	}
}
