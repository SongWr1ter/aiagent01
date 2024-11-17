using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextPanel : MonoBehaviour
{
    public TMP_Text dialogueText; // 用于显示对话框的Text组件
    private string fullText; // 要显示的完整文本
    public float delay = 0.1f; // 每个字符的显示间隔时间
    private bool isTyping = false; // 标记是否正在逐字显示文本
    private Coroutine typingCoroutine; // 存储协程引用
 
    private string currentText = "";
 
    public void StartTyping(string text)
    {
        if(isTyping) return;
        fullText = text;
        StartTyping();
    }
    void Update()
    {
        // 检测空格键是否被按下
        if (Input.GetKeyDown(KeyCode.Space) && isTyping)
        {
            StopTyping(); // 停止逐字显示
            dialogueText.text = fullText; // 立即显示全部文本
        }
    }
 
    public void StartTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(ShowText());
    }
 
    public void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }
    }
 
    IEnumerator ShowText()
    {
        isTyping = true;
        currentText = "";
        for (int i = 0; i <= fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            dialogueText.text = currentText;
            yield return new WaitForSeconds(delay);
        }
        isTyping = false;
    }
}
