using MimicSpace;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject winUI, lossUI;
    ThirdPersonController mPlayer;
    Movement mimicMove;
    // Start is called before the first frame update
    void Start()
    {
        mPlayer = FindObjectOfType<ThirdPersonController>();
        mimicMove = FindObjectOfType<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void win()
    {
        mimicMove.isDead = true;
        winUI.SetActive(true);
        lossUI.SetActive(false);
    }

    public void loss()
    {
        mPlayer.isDead = true;
        lossUI.SetActive(true);
        winUI.SetActive(false);
    }
}
