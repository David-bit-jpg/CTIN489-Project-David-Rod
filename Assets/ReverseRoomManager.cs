using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseRoomManager : MonoBehaviour
{
    [SerializeField] public GameObject[] stands;

    public GameObject objectToSpawn;
    public Vector3 spawnPosition;

    private bool hasSpawned = false;

    void Update()
    {
        if (!hasSpawned && AllStandsCorrect())
        {
            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            hasSpawned = true;
        }
    }

    bool AllStandsCorrect()
    {
        foreach (GameObject stand in stands)
        {
            Stand standScript = stand.GetComponent<Stand>();
            if (standScript != null)
            {
                if (!standScript.IsCorrect())
                {
                    return false;
                }
            }
        }
        return true;
    }
}
