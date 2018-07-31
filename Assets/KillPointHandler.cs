using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillPointHandler : MonoBehaviour {

    public handlePlayerSpawn playerSpawn;
    public handlePowerupSpawn powerupSpawn;

    public Text p1score;
    public Text p2score;

    public GameObject pauseScreen;

    private int p1score_val = 0;
    private int p2score_val = 0;

    public Text countdownMin;
    public Text countdownSec;
    public int minutes = 3;
    private int seconds = 0;

    private float timeStart;

    private bool pauseState = false;

	// Use this for initialization
	void Start () {
        reloadScoreDisplay();
        beginCountdown();
	}

    void Update()
    {
        checkPause();
    }

    public void updateScore(int player, int add)
    {
        if (player == 1) p1score_val += add;
        if (player == 2) p2score_val += add;
        reloadScoreDisplay();
    }

    void reloadScoreDisplay()
    {
        p1score.text = p1score_val.ToString();
        p2score.text = p2score_val.ToString();
    }

    void beginCountdown()
    {
        timeStart = Time.time;
        countdownMin.text = minutes.ToString();
        StartCoroutine(countdownRun());
    }

    IEnumerator countdownRun()
    {
        if (minutes <= 0 && seconds <= 0)
        {
            endGame();
            yield break;
        }

        reloadCountdownDisplay();
        yield return new WaitForSeconds(1);
        if (seconds == 0) { seconds = 59; minutes -= 1; }
        else { seconds -= 1; }
        StartCoroutine(countdownRun());
    }

    void reloadCountdownDisplay()
    {
        string sec_text = (seconds >= 10) ? seconds.ToString() : "0" + seconds.ToString();
        countdownMin.text = minutes.ToString();
        countdownSec.text = sec_text;
    }

    void endGame()
    {

    }

    void checkPause()
    {
        bool pause = Input.GetButtonUp("Pause");
        if (pause)
        {
            pauseState = !pauseState;
            handlePause();
        }
    }

    void handlePause()
    {
        if (pauseState) {
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
        }
        else {
            pauseScreen.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public Vector3 getSpawnPoint(Vector3 pos) { return playerSpawn.getFurthestSpawn(pos); }

    public void destroyPowerup(GameObject powerup) { powerupSpawn.destroyPowerup(powerup); }
}
