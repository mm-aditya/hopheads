using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class overall_data : MonoBehaviour {

    int p1 = 0;
    int p2 = 1;

    public GameObject map1;
    public GameObject map2;
    public GameObject map3;
    public GameObject endscreen;

    public GameObject menu;

    public int getP1()
    {
        return p1;
    }

    public int getP2()
    {
        return p2;
    }

    public void setP1(int val)
    {
        p1 = val;
    }

    public void setP2(int val)
    {
        p2 = val;
    }

    public void resetMap()
    {
        if (map == 1) map1.SetActive(false);
        if (map == 1) map1.SetActive(true);
        if (map == 2) map2.SetActive(false);
        if (map == 2) map2.SetActive(true);
        if (map == 3) map3.SetActive(false);
        if (map == 3) map3.SetActive(true);
    }
    

    public void quitMap()
    {
        map1.SetActive(false);
        map2.SetActive(false);
        map3.SetActive(false);

        Time.timeScale = 1;

        menu.SetActive(true);
    }

    int p1score;
    int p2score;
    public int get_p1score() { return p1score; }
    public int get_p2score() { return p2score; }

    public void endMatchScene(int p1, int p2)
    {
        p1score = p1;
        p2score = p2;

        map1.SetActive(false);
        map2.SetActive(false);
        map3.SetActive(false);

        Time.timeScale = 1;

        endscreen.SetActive(true);
    }

    private int map = 1;
    public void setMap(int m)
    {
        map = m;
    }
    public int getMap() { return map; }
}

