using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterveneButton : MonoBehaviour
{
    private Button button;
    private Vector3 initPos;
    private TMP_InputField inputField;
    public Transform target;
    public Sprite InterveneSprite;
    public Sprite LockSprite;
    public Sprite ReturnSprite;
    private bool toggle;
    private bool myLock;
    // Start is called before the first frame update
    void Start()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
        initPos = transform.position;
        button = GetComponent<Button>();
    }

    public void Toggle()
    {
        if(myLock) return;
        toggle = !toggle;
        if (toggle)
        {
            transform.DOMoveX(target.position.x, 0.5f);
            button.image.sprite = ReturnSprite;
        }
        else
        {
            transform.DOMoveX(initPos.x, 0.5f);
            button.image.sprite = InterveneSprite;
        }
    }

    public void SetLock(bool value)
    {
        myLock = value;
        if (value == true)
        {
            toggle = false;
            transform.DOMoveX(initPos.x, 0.5f);
        }
        
        if (myLock)
        {
            button.image.sprite = LockSprite;
        }
        else
        {
            button.image.sprite = InterveneSprite;
        }
    }

    public void Send()
    {
        if (!myLock && toggle)
        {
            string text = inputField.text;
            GameManager.Instance.PlayerInterveneSend(text);
            inputField.text = "";
            SetLock(true);
        }
        
    }
}
