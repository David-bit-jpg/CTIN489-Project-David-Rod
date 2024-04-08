using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room2Manager : MonoBehaviour
{
    [SerializeField] GameObject blockWall, Portal1, Portal2;
    DoorController dc;
    // Start is called before the first frame update
    void Start()
    {
        Portal1.SetActive(false);
        blockWall.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
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
