using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handlePlayerSpawn : MonoBehaviour {

    private List<Vector3> spawnPoints = new List<Vector3>();

	void Start () {
		foreach (Transform child in transform) {
            spawnPoints.Add(child.position);
        }
	}
	
    public Vector3 getFurthestSpawn(Vector3 pos)
    {
        Vector3 furthest = pos;

        foreach (Vector3 spawn in spawnPoints)
        {
            if (Vector3.Distance(spawn,pos) > Vector3.Distance(furthest, pos)) furthest = spawn;
        }
        return furthest;
    }	
}
