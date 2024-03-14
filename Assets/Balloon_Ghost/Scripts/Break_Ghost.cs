using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break_Ghost : MonoBehaviour
{
    public bool Is_Breaked = false;
    public GameObject ghost_normal;
    public GameObject ghost_Parts;
    public Animator ghost;
    int counter;

    public bool isPicked = false;
    BalloonSpawnerGood balloonSpawner;
    // Start is called before the first frame update
    void Start()
    {
        ghost_normal.SetActive(true);
        ghost_Parts.SetActive(false);
        balloonSpawner = FindObjectOfType<BalloonSpawnerGood>();
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y<= 2.2f && !isPicked)
            transform.position = new Vector3(transform.position.x, transform.position.y+0.002f, transform.position.z);
        if(Is_Breaked == true)
        {
            balloonSpawner.balloonCount --;
            TaskManager.Instance.UpdateTaskText();

            ghost_Parts.SetActive(true);
            ghost_Parts.transform.parent = null;
            ghost_normal.SetActive(false);
            transform.position = new Vector3(transform.position.x, transform.position.y-0.001f, transform.position.z);
            Destroy(gameObject);
        }
        
    }
    public void break_Ghost()
    {
        Is_Breaked = true;
    }
    public void play_anim()
    {
        counter += 1;
        if(counter == 2)
        {
            counter = 0;
            ghost.Play("idle");
        }
        else
        {
            ghost.Play("attack");
        }
    }
}
