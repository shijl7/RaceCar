using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    AudioSource scoreAudio;
    GameManager gameManager;
    bool addScore;
    void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        scoreAudio = GetComponent<AudioSource>();
    }

	private void OnTriggerEnter(Collider other)
	{
		if(!other.gameObject.transform.root.CompareTag("Player") || addScore)
        {
            return;
        }
        addScore = true;
        gameManager.UpdateScore(1);
        scoreAudio.Play();
	}
}
