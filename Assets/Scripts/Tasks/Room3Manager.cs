using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // 导入UI命名空间

public class Room3Manager : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();
    public Text gameOverText;

    int cnt = 0;

    private void Update()
    {
        if (CheckForBreakedBalloons())
        {
            cnt++;
        }
        if(cnt == 3)
        {
            GameOver();
        }
    }

    bool CheckForBreakedBalloons()
    {
        foreach (GameObject b in new List<GameObject>(gameObjects))
        {
            Break_Ghost balloonScript = b.GetComponent<Break_Ghost>();
            if (balloonScript != null && balloonScript.Is_Breaked)
            {
                gameObjects.Remove(b);
                return true;
            }
        }
        return false;
    }
    private void GameOver()
    {
        gameOverText.gameObject.SetActive(true);
        gameOverText.text = "Game Over!";
        Time.timeScale = 0;
    }
}
