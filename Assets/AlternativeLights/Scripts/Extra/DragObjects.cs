using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObjects : MonoBehaviour
{

    private Plane  groundPlane = new Plane(Vector3.up, Vector3.zero);
    [SerializeField]
    [Range(0,5)]
    private float hoverHeight = 2;
    [SerializeField]
    private GameObject displayObj;

    [SerializeField]
    private Rigidbody selectedObj;
    private Vector3 targetPos;
    [SerializeField]
    [Range(0,15)]
    private float maxSpeed = 2;

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.GetComponent<Rigidbody>())
                {
                    selectedObj = hit.transform.GetComponent<Rigidbody>();
                    selectedObj.useGravity = false;
                }

            }

        }
         else  if (Input.GetMouseButton(0))
        {
            Ray  ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entDis = 100.0f;
            if (groundPlane.Raycast(ray, out entDis))
            {
                targetPos = ray.GetPoint(entDis) + Vector3.up * hoverHeight;
                displayObj.transform.position = targetPos;
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);
            }
            if(selectedObj != null)
            {
               Vector3 moveDir = Vector3.ClampMagnitude((targetPos - selectedObj.transform.position) , maxSpeed);
                selectedObj.MovePosition(selectedObj.transform.position + ( moveDir * 3 * Time.deltaTime));
            }

        }else if (Input.GetMouseButtonUp(0) ) {
            if(selectedObj != null)
            {
                selectedObj.useGravity = true;
            }
            selectedObj = null;
        }
    }
}
