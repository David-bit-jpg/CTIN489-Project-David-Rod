using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : MonoBehaviour
{
    Rigidbody rb;
    public bool isPickedUp = false;
    public bool isPlaced = false;

    public Stand standPlaced = null;
    PlayerMovement player;
    Transform characterModel;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = FindObjectOfType<PlayerMovement>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isPickedUp)
        {
            rb.isKinematic = true;
            gameObject.transform.parent = player.CameraIntractPointer;

            Vector3 offset = player.CameraIntractPointer.forward * 1.5f;
            gameObject.transform.position = player.CameraIntractPointer.position + offset;
            
            gameObject.transform.forward = -player.CameraIntractPointer.forward;
        }
        else
        {
            rb.isKinematic = false;
            gameObject.transform.parent = null;
        }
    }


    public void PickUp()
    {
        isPickedUp = true;
        player.GetComponentInChildren<PlayerMovement>().heldVase = this;
        player.GetComponentInChildren<PlayerMovement>().isHoldingVase = true;
        this.characterModel = characterModel;
    }

    public void Drop(Vector3 pos)
    {
        isPickedUp = false;
        gameObject.transform.position = pos;
    }

    public void Place(Stand stand)
    {
        isPlaced = true;
        standPlaced = stand;
    }
}
