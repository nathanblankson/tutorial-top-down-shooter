using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;

    private void Start()
    {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0f;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }
}
