using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillPointHandler : MonoBehaviour {
    public handlePlayerSpawn playerSpawn;
    public handlePowerupSpawn powerupSpawn;

    public playerController p1Controller;
    public playerController p2Controller;

    public Text p1Score;
    public Text p2Score;

    public Transform p1ParryCooldown;
    public Transform p1DashCooldown;
    public Transform p2ParryCooldown;
    public Transform p2DashCooldown;
    public float shift = 0.15f;

    public GameObject pauseScreen;

    private int p1score_val = 0;
    private int p2score_val = 0;

    public Text countdownMin;
    public Text countdownSec;
    public int minutes = 3;
    private int seconds = 0;

    private float timeStart;

    private bool pauseState = false;

    public overall_data overall;
    public SpriteRenderer p1Sprite;
    public SpriteRenderer p2Sprite;

    public Sprite rabbit;
    public Sprite kangaroo;
    public Sprite robot;

    private AudioSource childsource;
    private AudioSource mainsource;

    public GameObject match_start;
    public GameObject match_over;
    public AudioClip startLevel;
    public AudioClip endLevel;

    // Use this for initialization
    void Awake () {
        childsource = GetComponentInChildren<AudioSource>();
        mainsource = GetComponent<AudioSource>();
    }
    private void Start()
    {
    }

    private void OnEnable()
    {
        Reset();
    }

    public void Reset()
    {
        int p1_icon = overall.getP1();
        int p2_icon = overall.getP2();
        if (p1_icon == 0) p1Sprite.sprite = rabbit;
        else if (p1_icon == 1) p1Sprite.sprite = kangaroo;
        else if (p1_icon == 2) p1Sprite.sprite = robot;
        if (p2_icon == 0) p2Sprite.sprite = rabbit;
        else if (p2_icon == 1) p2Sprite.sprite = kangaroo;
        else if (p2_icon == 2) p2Sprite.sprite = robot;

        p1score_val = 0;
        p2score_val = 0;
        reloadScoreDisplay();

        match_start.SetActive(false);
        match_over.SetActive(false);
        pauseState = false;
        handlePause();

        reloadScoreDisplay();

        minutes = 2;
        seconds = 0;

        StartCoroutine("startMatch");
    }

    IEnumerator startMatch()
    {
        print("start match");
        match_start.SetActive(true);
        childsource.PlayOneShot(startLevel);
        yield return new WaitForSeconds(1f);
        match_start.SetActive(false);
        beginCountdown();
    }

    void Update()
    {
        checkPause();
        updateCooldowns();
    }

    void updateCooldowns()
    {
        p1ParryCooldown.localPosition = new Vector3(0, -p1Controller.getParryCooldown() * shift, 0);
        p1DashCooldown.localPosition = new Vector3(0, -p1Controller.getDashCooldown() * shift, 0);
        p2ParryCooldown.localPosition = new Vector3(0, -p2Controller.getParryCooldown() * shift, 0);
        p2DashCooldown.localPosition = new Vector3(0, -p2Controller.getDashCooldown() * shift, 0);
    }

    public void updateScore(int player, int add)
    {
        if (player == 1) p1score_val += add;
        if (player == 2) p2score_val += add;
        reloadScoreDisplay();
    }

    void reloadScoreDisplay()
    {
        p1Score.text = p1score_val.ToString();
        p2Score.text = p2score_val.ToString();
    }

    void beginCountdown()
    {
        timeStart = Time.time;
        countdownMin.text = minutes.ToString();
        countdownSec.text = seconds.ToString();
        StartCoroutine(countdownRun());
    }

    IEnumerator countdownRun()
    {
        print("counting down: +"+minutes+"min "+seconds+"sec");
        reloadCountdownDisplay();

        if (minutes <= 0 && seconds <= 0)
        {
            endGame();
            yield break;
        }
        else
        {
            yield return new WaitForSeconds(1);
            if (seconds == 0 && minutes != 0) { seconds = 59; minutes -= 1; }
            else { seconds -= 1; }
            StartCoroutine(countdownRun());
        }
    }

    void reloadCountdownDisplay()
    {
        string sec_text = (seconds >= 10) ? seconds.ToString() : "0" + seconds.ToString();
        countdownMin.text = minutes.ToString();
        countdownSec.text = sec_text;
    }

    void endGame()
    {
        StartCoroutine("endMatch");
    }

    IEnumerator endMatch()
    {
        print("end match");
        mainsource.Pause();
        match_over.SetActive(true);
        childsource.PlayOneShot(endLevel);
        yield return new WaitForSeconds(1f);
        overall.endMatchScene(p1score_val, p2score_val);
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
