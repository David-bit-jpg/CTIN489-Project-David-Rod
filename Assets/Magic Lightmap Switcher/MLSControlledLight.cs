using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLSControlledLight : MonoBehaviour
{
    public enum ControlType
    {
        BlendByTime,
        SwitchByTime
    }

    public ControlType controlType;
    public float enableTime;
    public float disableTime;
    [SerializeField]
    public string lightGUID = Guid.NewGuid().ToString();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (controlType)
        {
            case ControlType.BlendByTime:

                break;
            case ControlType.SwitchByTime:
                break;
        }
    }
}
