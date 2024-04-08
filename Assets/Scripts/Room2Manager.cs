using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room2Manager : MonoBehaviour
{
    public enum MirrorTag
    {
        Mirror1,
        Mirror2,
        Mirror3
    }

    [SerializeField] GameObject blockWall, Portal1, Portal2;
    DoorController dc;
    ThirdPersonController thirdPersonController;
    public MirrorTag NearestMirror;
    [SerializeField] GameObject[] Mirrors = new GameObject[3];
    // Start is called before the first frame update
    void Start()
    {
        Portal1.SetActive(false);
        blockWall.SetActive(false);
        thirdPersonController = FindAnyObjectByType<ThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        FindClosestMirror();
    }

    void FindClosestMirror()
    {
        float closestMirrorDist = float.MaxValue;
        for(int i = 0; i < Mirrors.Length; i++)
        {
            float dist = Vector3.Distance(Mirrors[i].transform.position, transform.position);
            if (dist < closestMirrorDist)
            {
                closestMirrorDist = dist;
                switch (i)
                {
                    case 0:
                        NearestMirror = MirrorTag.Mirror1;
                        break;
                    case 1:
                        NearestMirror = MirrorTag.Mirror2;
                        break;
                    case 2:
                        NearestMirror = MirrorTag.Mirror3;
                        break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<ThirdPersonController>() != null)
        {
            blockWall.SetActive(true);
            Portal1.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>() != null)
        {
            blockWall.SetActive(false);
        }
    }

    public void Win()
    {
        Portal2.SetActive(false);
    }
}
