using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text scoreLabel;
    public Text timeLabel;
    public Car car;
    public Animator gameOverAnimator;

    public Text gameOverScoreLabel;
    public Text gameOverBestLabel;

    WorldGenerator worldGenerator;
    AudioSource gameOverMusic;
    float time;
    int score;
    bool gameOver;
    void Start()
    {
		gameOverMusic = GetComponent<AudioSource>();
		worldGenerator=GetComponent<WorldGenerator>();
		UpdateScore(0);
    }

    void Update()
    {
        UpdateTimer();
    }

    void UpdateTimer()
    {
        time += Time.deltaTime;
        int timer = (int)time;
        int seconds = timer % 60;
        int minutes = timer / 60;

        string secondRounded = ( seconds<10 ? "0" : "") + seconds;
        string minuteRounded = (minutes < 10 ? "0" : "") + minutes;

        timeLabel.text = minuteRounded + ":" + secondRounded;
    }

    public void UpdateScore(int points)
    {
        score += points;
        scoreLabel.text = score.ToString();
        worldGenerator.IncreaseItem();//增加难度
    }

    public void GameOver()
    {
        if (gameOver)
        {
            return;
        }
        SetScore();
        gameOverAnimator.SetTrigger("Game Over");

        car.FallApart();
        gameOverMusic.Play();
        gameOver = true;

		foreach (BasicMovement basicMovement in GameObject.FindObjectsOfType<BasicMovement>())
        {
            basicMovement.moveSpeed = 0;
            basicMovement.rotateSpeed = 0;
        }
    }

    void SetScore()
    {
        if(score>PlayerPrefs.GetInt("best"))
        {
            PlayerPrefs.SetInt(("best"),score);
        }
        gameOverScoreLabel.text = "Score:" + score;
        gameOverBestLabel.text = "Best:" + PlayerPrefs.GetInt("best");
    }

}
