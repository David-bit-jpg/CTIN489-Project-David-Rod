using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : MonoBehaviour
{
    Rigidbody rb;
    bool isPickedUp = false;
    GameObject player;
    Transform characterModel;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPickedUp)
        {
            rb.isKinematic = true;
            gameObject.transform.parent = player.transform;
            gameObject.transform.position = player.transform.position + characterModel.forward * 1.5f + new Vector3(0, 1, 0);
        }
        else
        {
            rb.isKinematic = false;
            gameObject.transform.parent = null;
        }
    }

    public void PickUp(GameObject player, Transform characterModel)
    {
        isPickedUp = true;
        player.GetComponent<PlayerMovement>().heldVase = this;
        this.player = player;
        this.characterModel = characterModel;
    }

    public void Drop()
    {
        isPickedUp = false;
    }
}
