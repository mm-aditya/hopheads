using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnHazard : MonoBehaviour {

    // Use this for initialization
    private List<Vector3> spawnPoints = new List<Vector3>();
    private float timeStart;
    public float timeWait = 1f;

    public GameObject hazard;

    void Start () {
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child.position);
        }

        timeStart = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
        float timeElapsed = Time.time - timeStart;
        float randomVal = Random.value;
        if (timeElapsed > timeWait) { if (randomVal > 0.85f) spawnRandomHazard(); }
    }

    void spawnRandomHazard()
    {
        //print("spawning hazard");
        //get random spawn location
        int randomIndex1 = Random.Range(0, spawnPoints.Count);
        Vector3 spawnPoint = spawnPoints[randomIndex1];

        GameObject new_hazard = Instantiate(hazard, spawnPoint, Quaternion.identity);

        timeStart = Time.time;
    }
}
