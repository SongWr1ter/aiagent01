using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoryPanel : MonoBehaviour
{
    public List<string> story = new List<string>();
    private TextPanel[] textPanels = new TextPanel[3];
    private CanvasGroup canvasGroup;
    bool flag = false;
    private Coroutine typingCoroutine; // 存储协程引用
    // Start is called before the first frame update
    void Start()
    {
        textPanels[0] = transform.GetChild(0).gameObject.GetComponent<TextPanel>();
        textPanels[1] = transform.GetChild(1).gameObject.GetComponent<TextPanel>();
        textPanels[2] = transform.GetChild(2).gameObject.GetComponent<TextPanel>();
        for (int i = 0; i < textPanels.Length; i++)
        {
            textPanels[i].StartTyping(" ");
        }
        canvasGroup = GetComponent<CanvasGroup>();
        
        typingCoroutine = StartCoroutine(sequenceShow());
    }

    private void Update()
    {
        if (flag)
        {
            if (Input.GetMouseButtonDown(0))
            {
                flag = false;
            }
        }
    }

    public void Skip()
    {
        SoundManager.PlayAudio("button");
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            flag = false;
        }
    }

    IEnumerator sequenceShow()
    {
        for (int i = 0; i < story.Count; i++)
        {
            textPanels[i].StartTyping(story[i]);
            flag = true;
            while(flag) yield return new WaitForSeconds(0.2f);
        }
    }
}
