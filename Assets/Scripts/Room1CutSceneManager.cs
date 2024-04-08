using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room1CutSceneManager : MonoBehaviour
{
    [SerializeField] Animator deadBodyAnimator;
    [SerializeField] DoorController door;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (door.isOpened && !deadBodyAnimator.gameObject.activeSelf)
        {
            deadBodyAnimator.gameObject.SetActive(true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ConstantForce>())
        {
            deadBodyAnimator.SetBool("Hit", true);
        }

    }
}
