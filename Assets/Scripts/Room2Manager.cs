using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room2Manager : MonoBehaviour
{
    [SerializeField] GameObject Door1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DisableDoor()
    {
        DoorController dc = Door1.GetComponent<DoorController>();
        if (dc)
        {
            dc.enabled = false;
        }
    }
}
