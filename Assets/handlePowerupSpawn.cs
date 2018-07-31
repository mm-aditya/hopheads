using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class handlePowerupSpawn : MonoBehaviour
{
    private List<GameObject> powerups;
    //private List<Vector3> spawnPoints = new List<Vector3>();
    private List<Vector3> unoccupiedSpawns = new List<Vector3>();
    private List<Vector3> occupiedSpawns = new List<Vector3>();
    private int spawnCount = 0;
    public int maxSpawnCount = 2;

    private float timeStart;
    public float timeWait = 5.0f;

    private Dictionary<GameObject, Vector3> powerupSpawned = new Dictionary<GameObject, Vector3>();

    void Start()
    {
        //setup
        powerups = Resources.LoadAll("powerups", typeof(GameObject)).Cast<GameObject>().ToList();
        foreach (Transform child in transform) unoccupiedSpawns.Add(child.position);

        spawnRandomPowerup();
        spawnRandomPowerup();

        timeStart = Time.time;
    }

    private void Update()
    {
        float timeElapsed = Time.time - timeStart;
        if (spawnCount < maxSpawnCount)
        {
            if (timeElapsed > timeWait) spawnRandomPowerup();
        }
        else timeStart = Time.time;
    }

    void spawnRandomPowerup()
    {
        if (powerups.Count == 0) return; //no powerups loaded
        if (unoccupiedSpawns.Count == 0) return; //no more place to spawn

        //get random spawn location
        int randomIndex1 = Random.Range(0, unoccupiedSpawns.Count);
        Vector3 spawnPoint = unoccupiedSpawns[randomIndex1];

        //get random powerup & spawn it
        int randomIndex2 = Random.Range(0, powerups.Count);
        GameObject powerup = Instantiate(powerups[randomIndex2], spawnPoint, Quaternion.identity);
        powerup.name = powerups[randomIndex2].name;

        //update lists
        powerupSpawned[powerup] = spawnPoint;
        occupiedSpawns.Add(unoccupiedSpawns[randomIndex1]);
        unoccupiedSpawns.RemoveAt(randomIndex1);
        spawnCount++;
        timeStart = Time.time;
    }

    public void destroyPowerup(GameObject powerup)
    {
        Vector3 spawnPoint = powerupSpawned[powerup];

        powerupSpawned.Remove(powerup);
        occupiedSpawns.Remove(spawnPoint);
        unoccupiedSpawns.Add(spawnPoint);
        spawnCount--;

        Destroy(powerup);
    }
}

