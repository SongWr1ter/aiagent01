using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nDebuger : MonoBehaviour
{
    public StatePanelItem PlayerPanelItem;
    public StatePanelItem AIPanelItem;
    public Slime A;
    public Slime B;
    public TextPanel TextPanel;
    [TextArea]
    public string TextArea;
    private void Start()
    {
        PlayerPanelItem.Init(A);
        AIPanelItem.Init(B);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            //GameManager.Instance.debugattack(A,B);
            //TextPanel.StartTyping(TextArea);
            //GameManager.Instance.Setup();
            GameManager.Instance.Action(A, B);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            //GameManager.Instance.debugattack(A,B);
            //TextPanel.StartTyping(TextArea);
            //GameManager.Instance.Setup();
            GameManager.Instance.Action(B, A);
        }
    }
}
