using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCam : MonoBehaviour
{


    [Header("Controls")]
    [SerializeField]
    private Transform yawPivot;
    [SerializeField]
    private Transform pitchPivot;

    private float currentYaw = 2;
    [SerializeField]
    private float currentPitch;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float sen = 5;


    private void Start()
    {
        currentPitch = pitchPivot.localEulerAngles.x;
        currentYaw = yawPivot.localEulerAngles.y;
    }
    private void Update()
    {


        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentYaw += Input.GetAxis("Mouse X") * sen;
            yawPivot.localEulerAngles = new Vector3(0, currentYaw, 0);

            currentPitch -= Input.GetAxis("Mouse Y") * sen;

            currentPitch = Mathf.Clamp(currentPitch, 10, 80);
            pitchPivot.localEulerAngles = new Vector3(currentPitch, 0, 0);

        }


    }
    public void ResetRotation()
    {
        currentPitch = 0;
        currentYaw = 0;

        yawPivot.localEulerAngles = new Vector3(0, currentYaw, 0);
        pitchPivot.localEulerAngles = new Vector3(currentPitch, 0, 0);
    }


}
