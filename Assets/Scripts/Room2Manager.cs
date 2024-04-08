using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room2Manager : MonoBehaviour
{
    [SerializeField] GameObject Door1, Portal1;
    DoorController dc;
    // Start is called before the first frame update
    void Start()
    {
        Portal1.SetActive(false);
        dc = Door1.GetComponentInChildren<DoorController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DisableDoor()
    {
        
        if (dc)
        {
            dc.enabled = false;
        }
    }

    void EnableDoor()
    {
        if (dc)
        {
            dc.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<ThirdPersonController>() != null)
        {
            dc.ToggleDoor();
            DisableDoor();
            Portal1.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>() != null)
        {
            EnableDoor();
        }
    }
}
