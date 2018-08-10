using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pauseControls : MonoBehaviour {


    public overall_data overall;


    AudioSource source;
    public AudioClip switching;
    public AudioClip click;
    private bool releasev = false;

    private int selected = 0;
    public Button resume;
    public Button restart;
    public Button quit;

    public KillPointHandler main;

    // Use this for initialization
    void Start () {
        source = GetComponent<AudioSource>();	
	}

    private void OnEnable()
    {
        selected = 0;
        resume.OnSelect(null);

        restart.OnDeselect(null);
        quit.OnDeselect(null);
    }

    // Update is called once per frame
    void Update () {
        checkScroll();
	}

    void checkScroll()
    {
        float v1 = Input.GetAxis("p1Vertical");
        float v2 = Input.GetAxis("p2Vertical");

        bool a1 = Input.GetButtonDown("p1Select");
        bool a2 = Input.GetButtonDown("p2Select");

        //scroll down
        if ((v1 == -1 || v2 == -1) && releasev == true)
        {
            source.PlayOneShot(switching);
            releasev = false;
            scroll("down");
        }
        //scroll up
        else if ((v1 == 1 || v2 == 1) && releasev == true)
        {
            source.PlayOneShot(switching);
            releasev = false;
            scroll("up");
        }
        else if (v1 == 0 && v2 == 0) releasev = true;

        //click
        if (a1 || a2)
        {
            source.PlayOneShot(click);

            bool p1 = (a1) ? true : false;
            select();
        }
    }

    void scroll(string direction)
    {
        if (direction == "down")
        {
            if (selected == 0) { resume.OnDeselect(null); restart.OnSelect(null); selected = 1; }
            else if (selected == 1) { restart.OnDeselect(null); quit.OnSelect(null); selected = 2; }
        }
        else if (direction == "up")
        {
            if (selected == 2) { quit.OnDeselect(null); restart.OnSelect(null); selected = 1; }
            else if (selected == 1) { restart.OnDeselect(null); resume.OnSelect(null); selected = 0; }
        }
    }

    void select()
    {
        if (selected == 0) closePause();
        if (selected == 1) { print("restart");  overall.resetMap(); }
        if (selected == 2) overall.quitMap();
    }

    void closePause()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

}
