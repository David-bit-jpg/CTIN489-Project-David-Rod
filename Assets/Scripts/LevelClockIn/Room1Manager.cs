using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Diagnostics;

public class Room1Manager : MonoBehaviour
{
    [SerializeField] GameObject base1, base2, base3;
    [SerializeField] GameObject vase1, vase2, vase3;
    [SerializeField] GameObject Key;
    [SerializeField] Lever lever;
    [SerializeField] DoorController door;
    public bool isMatch = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector2Dist(base1.transform.position, vase1.transform.position) < .5f
        && Vector2Dist(base2.transform.position, vase2.transform.position) < .5f
        && Vector2Dist(base3.transform.position, vase3.transform.position) < .5f)
        {
            isMatch = true;
        }
        if (isMatch && lever.GetIsOn())
        {
            if(Key != null && Key.gameObject.transform.parent != null)
            {
                Key.GetComponent<Rigidbody>().isKinematic = false;
                Key.gameObject.transform.parent = null;
            }
        }
        else if(!isMatch && door.isOpened)
        {
            lever.startRotation = lever.openRotation;
            lever.Close();
        }
        SwitchGravity(lever.GetIsOn());
    }

    float Vector2Dist(Vector3 pos1, Vector3 pos2)
    {
        Vector2 pos1Vec2 = new Vector2(pos1.x, pos1.z);
        Vector2 pos2Vec2 = new Vector2(pos2.x, pos2.z);

        return Vector2.Distance(pos1Vec2, pos2Vec2);
    }


    void SwitchGravity(bool turnOn)
    {
        if (turnOn)
        {
            vase1.GetComponent<ConstantForce>().force = new Vector3(0, 10, 0);
            vase2.GetComponent<ConstantForce>().force = new Vector3(0, 10, 0);
            vase3.GetComponent<ConstantForce>().force = new Vector3(0, 10, 0);
        }
        else
        {
            vase1.GetComponent<ConstantForce>().force = new Vector3(0, -10, 0);
            vase2.GetComponent<ConstantForce>().force = new Vector3(0, -10, 0);
            vase3.GetComponent<ConstantForce>().force = new Vector3(0, -10, 0);
        }
    }
}
