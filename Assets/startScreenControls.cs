using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class startScreenControls : MonoBehaviour
{
    public GameObject menu;
    public overall_data overall;

    public GameObject mainmenu;
    public GameObject controlsmenu;
    public GameObject chars;
    public GameObject maps;

    public GameObject level1;
    public GameObject level2;
    public GameObject level3;

    public Button play;
    public Button controls;
    public Button quit;

    public Button controlsBack;

    public Button char1;
    public Button char2;
    public Button char3;
    public Animator char1_anim;
    public Animator char2_anim;
    public Animator char3_anim;
    public Button charBack;
    public Button charNext;
    private int p1_choice = 0;
    private int p2_choice = 0;

    public Button mapBack;
    public Button map1;
    public Button map2;
    public Button map3;
    public Button mapNext;

    private int selected;
    private bool releasev = false;
    private bool releaseh = false;

    private string screen;

    public AudioClip switching;
    public AudioClip click;
    private AudioSource source;

    public GameObject map1BG;
    public GameObject map2BG;
    public GameObject map3BG;

    // Use this for initialization
    void Start()
    {
        //setup
        source = GetComponent<AudioSource>();
        hideAll();
        startMainmenu();
    }

    private void OnEnable()
    {
        //setup
        source = GetComponent<AudioSource>();
        hideAll();

        map1BG.SetActive(true);
        map2BG.SetActive(false);
        map3BG.SetActive(false);

        startMainmenu();
    }

    // Update is called once per frame
    void Update()
    {
        checkScroll();
    }
    
    void hideAll()
    {
        mainmenu.SetActive(false);
        controlsmenu.SetActive(false);
        chars.SetActive(false);
        maps.SetActive(false);
        level1.SetActive(false);
        level2.SetActive(false);
        level3.SetActive(false);
    }

    void checkScroll()
    {
        float v1 = Input.GetAxis("p1Vertical");
        float v2 = Input.GetAxis("p2Vertical");
        float h1 = Input.GetAxis("p1Horizontal");
        float h2 = Input.GetAxis("p2Horizontal");

        bool a1 = Input.GetButtonDown("p1Select");
        bool a2 = Input.GetButtonDown("p2Select");

        //scroll down
        if ((v1 == -1 || v2 == -1) && releasev == true)
        {
            source.PlayOneShot(switching);
            releasev = false;
            if (screen == "mainmenu") scrollMainmenu("down");
        }
        //scroll up
        else if ((v1 == 1 || v2 == 1) && releasev == true)
        {
            source.PlayOneShot(switching);
            releasev = false;
            if (screen == "mainmenu") scrollMainmenu("up");
        }
        else if (v1 == 0 && v2 == 0) releasev = true;

        //scroll left
        if ((h1 == -1 || h2 == -1) && releaseh == true)
        {
            source.PlayOneShot(switching);
            releaseh = false;
            if (screen == "chars") scrollChars("left", h1 == -1);
            if (screen == "maps") scrollMaps("left");
        }
        //scroll right
        else if ((h1 == 1 || h2 == 1) && releaseh == true)
        {
            source.PlayOneShot(switching);
            releaseh = false;
            if (screen == "chars") scrollChars("right", h1 == 1);
            if (screen == "maps") scrollMaps("right");
        }
        else if (h1 == 0 && h2 == 0) releaseh = true;

        //click
        if (a1 || a2)
        {
            source.PlayOneShot(click);

            bool p1 = (a1) ? true : false;
            if (screen == "mainmenu") selectMainmenu();
            else if (screen == "controls") selectControls();
            else if (screen == "chars") selectChars(p1);
            else if (screen == "maps") selectMaps();
        }
    }

    void startMainmenu(int sel = 0)
    {
        selected = sel;
        mainmenu.SetActive(true);
        screen = "mainmenu";

        play.OnDeselect(null);
        controls.OnDeselect(null);
        quit.OnDeselect(null);

        if (sel == 0) play.OnSelect(null);
        else if (sel == 1) controls.OnSelect(null);
        else if (sel == 2) quit.OnSelect(null);
    }

    void startControls()
    {
        controlsmenu.SetActive(true);
        screen = "controls";

        controlsBack.OnSelect(null);
    }

    void startMaps()
    {
        maps.SetActive(true);
        screen = "maps";

        selected = 1;
        map1.OnSelect(null);
        mapBack.OnDeselect(null);
        mapNext.OnDeselect(null);
    }

    void startChars()
    {
        chars.SetActive(true);
        screen = "chars";

        p1_choice = 0;
        p2_choice = 1;
        resetCharAnim(); //hard coded reset
        setScrollCharBtn(p1_choice, true, 1);
        setScrollCharBtn(p2_choice, true, 2);
        chooseChar(p1_choice, 1);
        chooseChar(p2_choice, 2);
    }

    void scrollMainmenu(string direction)
    {
        if (direction == "down")
        {
            if (selected == 0) { play.OnDeselect(null); controls.OnSelect(null); selected = 1; }
            else if (selected == 1) { controls.OnDeselect(null); quit.OnSelect(null); selected = 2; }
        }
        else if (direction == "up")
        {
            if (selected == 2) { quit.OnDeselect(null); controls.OnSelect(null); selected = 1; }
            else if (selected == 1) { controls.OnDeselect(null); play.OnSelect(null); selected = 0; }
        }
    }

    void scrollChars(string direction, bool p1)
    {
        //unset highlighted animation
        if (p1 && p1_choice != -1 && p1_choice != 3) getCharAnim(p1_choice).SetBool("isHighlighted", false);
        if (!p1 && p2_choice != -1 && p2_choice != 3) getCharAnim(p2_choice).SetBool("isHighlighted", false);

        if (p1 && direction == "left")
        {
            if (p1_choice == 0)
            {
                setScrollCharBtn(0, false, 1); //descroll btn 0
                setScrollCharBtn(-1, true, 1); //scroll btn -1
                p1_choice = -1;
            }
            else if (p1_choice == 1)
            {
                setScrollCharBtn(1, false, 1); //descroll btn 1
                setScrollCharBtn(0, true, 1); //scroll btn 0
                p1_choice = 0;
            }
            else if (p1_choice == 2)
            {
                setScrollCharBtn(2, false, 1); //descroll btn 2
                setScrollCharBtn(1, true, 1); //scroll btn 1
                p1_choice = 1;
            }
            else if (p1_choice == 3)
            {
                setScrollCharBtn(3, false, 1); //descroll btn 3
                setScrollCharBtn(2, true, 1); //scroll btn 2
                p1_choice = 2;
            }
        }
        else if (p1 && direction == "right")
        {
            if (p1_choice == -1)
            {
                setScrollCharBtn(-1, false, 1);
                setScrollCharBtn(0, true, 1);
                p1_choice = 0;
            }
            else if (p1_choice == 0)
            {
                setScrollCharBtn(0, false, 1);
                setScrollCharBtn(1, true, 1);
                p1_choice = 1;
            }
            else if (p1_choice == 1)
            {
                setScrollCharBtn(1, false, 1);
                setScrollCharBtn(2, true, 1);
                p1_choice = 2;
            }
            else if (p1_choice == 2)
            {
                setScrollCharBtn(2, false, 1);
                setScrollCharBtn(3, true, 1);
                p1_choice = 3;
            }
        }

        if (!p1 && direction == "left")
        {
            if (p2_choice == 0)
            {
                setScrollCharBtn(0, false, 2); //descroll btn 0
                setScrollCharBtn(-1, true, 2); //scroll btn -1
                p2_choice = -1;
            }
            else if (p2_choice == 1)
            {
                setScrollCharBtn(1, false, 2); //descroll btn 1
                setScrollCharBtn(0, true, 2); //scroll btn 0
                p2_choice = 0;
            }
            else if (p2_choice == 2)
            {
                setScrollCharBtn(2, false, 2); //descroll btn 2
                setScrollCharBtn(1, true, 2); //scroll btn 1
                p2_choice = 1;
            }
            else if (p2_choice == 3)
            {
                setScrollCharBtn(3, false, 2); //descroll btn 3
                setScrollCharBtn(2, true, 2); //scroll btn 2
                p2_choice = 2;
            }
        }
        else if (!p1 && direction == "right")
        {
            if (p2_choice == -1)
            {
                setScrollCharBtn(-1, false, 2);
                setScrollCharBtn(0, true, 2);
                p2_choice = 0;
            }
            else if (p2_choice == 0)
            {
                setScrollCharBtn(0, false, 2);
                setScrollCharBtn(1, true, 2);
                p2_choice = 1;
            }
            else if (p2_choice == 1)
            {
                setScrollCharBtn(1, false, 2);
                setScrollCharBtn(2, true, 2);
                p2_choice = 2;
            }
            else if (p2_choice == 2)
            {
                setScrollCharBtn(2, false, 2);
                setScrollCharBtn(3, true, 2);
                p2_choice = 3;
            }
        }

        //set highlighted animation
        if (p1 && p1_choice != -1 && p1_choice != 3) getCharAnim(p1_choice).SetBool("isHighlighted", true);
        if (!p1 && p2_choice != -1 && p2_choice != 3) getCharAnim(p2_choice).SetBool("isHighlighted", true);
    }

    void setScrollCharBtn(int charNum, bool state, int pNum)
    {
        Button charBtn = (charNum == -1) ? charBack : (charNum == 0) ? char1 : (charNum == 1) ? char2 : (charNum == 2) ? char3 : charNext;
        int pScroll = (pNum == 1) ? 3 : 4;

        //set pScroll state
        if (charNum != -1 && charNum != 3) getCharObj(charNum).GetChild(pScroll).GetComponent<Text>().enabled = state;
        else //set button state only for back & next buttons
        {
            if (state) charBtn.OnSelect(null);
            else
            {
                if (pNum == 1 && charNum != p2_choice) charBtn.OnDeselect(null);
                else if (pNum == 2 && charNum != p1_choice) charBtn.OnDeselect(null);
            }
        }
    }

    void scrollMaps(string direction)
    {
        if (direction == "left")
        {
            if (selected == 1) {
                map1.OnDeselect(null);
                mapBack.OnSelect(null);
                selected = 0;
            }
            else if (selected == 2) {
                map2.OnDeselect(null);
                map1.OnSelect(null);
                selected = 1;
                map1BG.SetActive(true);
                map2BG.SetActive(false);
                map3BG.SetActive(false);
            }
            else if (selected == 3) {
                map3.OnDeselect(null);
                map2.OnSelect(null);
                selected = 2;
                map1BG.SetActive(false);
                map2BG.SetActive(true);
                map3BG.SetActive(false);
            }
        }
        else if (direction == "right")
        {
            print("before: "+selected);
            if (selected == 0) {
                mapBack.OnDeselect(null);
                map1.OnSelect(null);
                selected = 1;
                map1BG.SetActive(true);
                map2BG.SetActive(false);
                map3BG.SetActive(false);
            }
            else if (selected == 1) {
                map1.OnDeselect(null);
                map2.OnSelect(null);
                selected = 2;
                map1BG.SetActive(false);
                map2BG.SetActive(true);
                map3BG.SetActive(false);
            }
            else if (selected == 2) {
                map2.OnDeselect(null);
                map3.OnSelect(null);
                selected = 3;
                map1BG.SetActive(false);
                map2BG.SetActive(false);
                map3BG.SetActive(true);
            }
            print("after: " + selected);
        }
    }

    void selectMainmenu()
    {
        if (selected == 0) {
            play.OnDeselect(null);
            mainmenu.SetActive(false);

            startChars();
        }
        else if (selected == 1) {
            controls.OnDeselect(null);
            mainmenu.SetActive(false);

            startControls();
        }
        else if (selected == 2) {
            //quit.OnDeselect(null);
            //Application.Quit();
        }
    }

    void selectControls()
    {
        controlsBack.OnDeselect(null);
        controlsmenu.SetActive(false);

        startMainmenu(1);
    } 

    void selectChars(bool player)
    {
        if ((player && p1_choice == -1) || (!player && p2_choice == -1)) {
            charBack.OnDeselect(null);
            chars.SetActive(false);
            startMainmenu(0);
        }
        else if ((player && p1_choice == 3) || (!player && p2_choice == 3)) {
            charNext.OnDeselect(null);
            chars.SetActive(false);

            overall.setP1(p1_char);
            overall.setP2(p2_char);
            startMaps();
        }
        else
        {
            if (player) chooseChar(p1_choice, 1);
            else chooseChar(p2_choice, 2);
        }
    }

    void selectMaps()
    {
        if (selected == 0) {
            mapBack.OnDeselect(null);

            maps.SetActive(false);
            startChars();
        }
        if (selected == 1) {
            map1.OnDeselect(null);

            chars.SetActive(false);
            level1.SetActive(true);

            overall.setMap(1);
            mainmenu.SetActive(true);
            menu.SetActive(false);
        }
        if (selected == 2)
        {
            map2.OnDeselect(null);

            chars.SetActive(false);
            level2.SetActive(true);

            overall.setMap(2);
            mainmenu.SetActive(true);
            menu.SetActive(false);
        }
        if (selected == 3)
        {
            map2.OnDeselect(null);

            chars.SetActive(false);
            level3.SetActive(true);

            overall.setMap(3);
            mainmenu.SetActive(true);
            menu.SetActive(false);
        }
    }

    bool char1_selected = false;
    bool char2_selected = false;
    bool char3_selected = false;
    int p1_selected = -1;
    int p2_selected = -1;
    int p1_char = 0;
    int p2_char = 1;

    void chooseChar(int character, int player)
    {
        if (character == p1_selected || character == p2_selected) return;

        //unselect previous selection (if any)
        if ((player == 1 && p1_selected != -1) || (player == 2 && p2_selected != -1)) {
            //unset char bool
            if (player == 1) setCharBool(p1_selected, false);
            else setCharBool(p2_selected, false);
            //unselect animation
            if (player == 1) getCharAnim(p1_selected).SetBool("isSelected", false);
            else getCharAnim(p2_selected).SetBool("isSelected", false);
            //unselect player icon
            if (player == 1) getCharObj(p1_selected).GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
            else getCharObj(p2_selected).GetChild(2).GetComponent<SpriteRenderer>().enabled = false;
            //unselect button
            if (player == 1) getCharBtn(p1_selected).OnDeselect(null);
            else getCharBtn(p2_selected).OnDeselect(null);
        }

        //set player
        if (player == 1) p1_selected = character;
        else p2_selected = character;
        //set char bool
        setCharBool(character, true);
        //select animation
        getCharAnim(character).SetBool("isSelected", true);
        if (player == 1) p1_char = character;
        else p2_char = character;
        //select player icon
        if (player == 1) getCharObj(character).GetChild(1).GetComponent<SpriteRenderer>().enabled = true;
        else getCharObj(character).GetChild(2).GetComponent<SpriteRenderer>().enabled = true;
        //select button
        if (player == 1) getCharBtn(p1_selected).OnSelect(null);
        else getCharBtn(p2_selected).OnSelect(null);
    }

    private Animator getCharAnim(int character)
    {
        if (character == 0) return char1_anim;
        if (character == 1) return char2_anim;
        if (character == 2) return char3_anim;
        return null;
    }

    private void resetCharAnim()
    {
        char1_anim.SetBool("isSelected", true);
        char2_anim.SetBool("isSelected", true);
        char3_anim.SetBool("isSelected", false);
        char1_anim.SetBool("isHighlighted", false);
        char2_anim.SetBool("isHighlighted", false);
        char3_anim.SetBool("isHighlighted", false);

        p1_char = 0;
        p2_char = 1;
    }

    private Button getCharBtn(int character)
    {
        if (character == 0) return char1;
        if (character == 1) return char2;
        if (character == 2) return char3;
        return null;
    }

    private Transform getCharObj(int character)
    {
        if (character == 0) return char1.transform;
        if (character == 1) return char2.transform;
        if (character == 2) return char3.transform;
        return null;
    }

    private void setCharBool(int character, bool state)
    {
        if (character == 0) char1_selected = state;
        if (character == 1) char2_selected = state;
        if (character == 2) char3_selected = state;
    }
}
