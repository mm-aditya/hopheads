using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class endmatch_controller : MonoBehaviour {

    public GameObject rabbitleft;
    public GameObject rabbitright;
    public GameObject robotleft;
    public GameObject robotright;
    public GameObject kangarooleft;
    public GameObject kangarooright;

    public GameObject winText;

    public Button returnMain;

    private List<GameObject> leftChars;
    private List<GameObject> rightChars;

    private Vector3 leftOrigin;
    private Vector3 rightOrigin;

    public overall_data overall;

    private int p1score;
    private int p2score;

    private GameObject p1_obj;
    private GameObject p2_obj;

    private AudioSource source;
    public AudioClip click;
    public AudioClip dash;

    public GameObject mainmenu;

    public GameObject map1BG;
    public GameObject map2BG;
    public GameObject map3BG;

    // Use this for initialization
    private void Awake()
    {
        leftOrigin = rabbitleft.transform.localPosition;
        rightOrigin = rabbitright.transform.localPosition;
        leftChars = new List<GameObject>() {rabbitleft, robotleft, kangarooleft};
        rightChars = new List<GameObject>() { rabbitright, robotright, kangarooright};

        source = GetComponent<AudioSource>();
    }

    void OnEnable () {
        resetChars();

        //get map
        int map = overall.getMap();
        map1BG.SetActive(false);
        map2BG.SetActive(false);
        map3BG.SetActive(false);
        if (map == 1) map1BG.SetActive(true);
        else if (map == 2) map2BG.SetActive(true);
        else if (map == 3) map3BG.SetActive(true);

        //get scores
        p1score = overall.get_p1score();
        p2score = overall.get_p2score();

        //activate characters
        activeChar(overall.getP1(), 1);
        activeChar(overall.getP2(), 2);

        setChar(overall.getP1(), 1);
        setChar(overall.getP2(), 2);

        winText.SetActive(false);
        returnMain.gameObject.SetActive(false);
        StartCoroutine("winner");
    }

    public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.localPosition;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.localPosition = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.localPosition = end;
    }

    IEnumerator winner()
    {
        Vector3 newPosL = leftOrigin;
        newPosL.x += 7;
        Vector3 newPosR = rightOrigin;
        newPosR.x -= 7.5f;

        StartCoroutine(MoveOverSeconds(p1_obj, newPosL, 1.2f));
        StartCoroutine(MoveOverSeconds(p2_obj, newPosR, 1.2f));
        source.PlayOneShot(dash);

        yield return new WaitForSeconds(1f);

        winText.SetActive(true);
        Text winTextDisplay = winText.GetComponent<Text>();

        if (p1score > p2score) //p1 win
        {
            winChar(overall.getP1(), 1);
            loseChar(overall.getP2(), 2);
            winTextDisplay.text = "P1 WON!";
        }
        else if (p2score > p1score) //p2 win
        {
            winChar(overall.getP2(), 2);
            loseChar(overall.getP1(), 1);
            winTextDisplay.text = "P2 WON!";
        } else //tie
        {
            winChar(overall.getP1(), 1);
            winChar(overall.getP2(), 2);
            winTextDisplay.text = "ITS A TIE!";
        }

        returnMain.gameObject.SetActive(true);
        returnMain.OnSelect(null);
    }

    void resetChars()
    {
        foreach (GameObject chara in leftChars) {
            chara.transform.localPosition = leftOrigin;
            chara.SetActive(false);
        }
        foreach (GameObject chara in rightChars)
        {
            chara.transform.localPosition = rightOrigin;
            chara.SetActive(false);
        }
    }

    void setChar(int cnum, int pnum)
    {
        if (pnum == 1)
        {
            if (cnum == 0) p1_obj = rabbitleft;
            else if (cnum == 1) p1_obj = kangarooleft;
            else if (cnum == 2) p1_obj = robotleft;
        }
        else if (pnum == 2)
        {
            if (cnum == 0) p2_obj = rabbitright;
            else if (cnum == 1) p2_obj = kangarooright;
            else if (cnum == 2) p2_obj = robotright;
        }
    }

    void activeChar(int cnum, int pnum)
    {
        if (pnum == 1)
        {
            if (cnum == 0) rabbitleft.SetActive(true);
            else if (cnum == 1) kangarooleft.SetActive(true);
            else if (cnum == 2) robotleft.SetActive(true);
        }
        else if (pnum == 2)
        {
            if (cnum == 0) rabbitright.SetActive(true);
            else if (cnum == 1) kangarooright.SetActive(true);
            else if (cnum == 2) robotright.SetActive(true);
        }
    }

    void winChar(int cnum, int pnum)
    {
        if (pnum == 1)
        {
            if (cnum == 0) rabbitleft.GetComponent<Animator>().SetBool("win",true);
            else if (cnum == 1) kangarooleft.GetComponent<Animator>().SetBool("win", true);
            else if (cnum == 2) robotleft.GetComponent<Animator>().SetBool("win", true);
        }
        else if (pnum == 2)
        {
            if (cnum == 0) rabbitright.GetComponent<Animator>().SetBool("win", true);
            else if (cnum == 1) kangarooright.GetComponent<Animator>().SetBool("win", true);
            else if (cnum == 2) robotright.GetComponent<Animator>().SetBool("win", true);
        }
    }

    void loseChar(int cnum, int pnum)
    {
        if (pnum == 1)
        {
            if (cnum == 0) rabbitleft.GetComponent<Animator>().SetBool("lose", true);
            else if (cnum == 1) kangarooleft.GetComponent<Animator>().SetBool("lose", true);
            else if (cnum == 2) robotleft.GetComponent<Animator>().SetBool("lose", true);
        }
        else if (pnum == 2)
        {
            if (cnum == 0) rabbitright.GetComponent<Animator>().SetBool("lose", true);
            else if (cnum == 1) kangarooright.GetComponent<Animator>().SetBool("lose", true);
            else if (cnum == 2) robotright.GetComponent<Animator>().SetBool("lose", true);
        }
    }

    // Update is called once per frame
    void Update () {
        checkClick();
	}

    void checkClick()
    {
        bool a1 = Input.GetButtonDown("p1Select");
        bool a2 = Input.GetButtonDown("p2Select");

        //click
        if ((a1 || a2) && (returnMain.gameObject.activeSelf == true))
        {
            source.PlayOneShot(click);

            mainmenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}

