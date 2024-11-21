using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    private TMP_Text text;
    private CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponentInChildren<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        show(false);
    }

    public void OnGameOver(bool win)
    {
        show(true);
        if (win)
        {
            text.text = "YOU WIN";
        }
        else
        {
            text.text = "YOU LOSE";
        }
    }

    public void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void OnReload()
    {
        SceneManager.LoadScene(0);
    }

    void show(bool flag)
    {
        canvasGroup.alpha = flag ? 1 : 0;
        canvasGroup.interactable = flag;
        canvasGroup.blocksRaycasts = flag;
    }
}
