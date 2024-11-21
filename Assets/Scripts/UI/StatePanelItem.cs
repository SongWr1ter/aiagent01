using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatePanelItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text NameText;
    private TMP_Text HPText;
    private Image HPImage;
    private TMP_Text typeText;
    [SerializeField]
    private bool isPlayer;

    private int maxHP;
    // Start is called before the first frame update
    void Awake()
    {
        NameText = transform.GetChild(2).GetComponent<TMP_Text>();
        HPText = transform.GetChild(1).GetComponent<TMP_Text>();
        HPImage = transform.GetChild(0).GetComponent<Image>();
        typeText = transform.GetChild(3).GetComponent<TMP_Text>();

        GameManager.OnSlimeHpChanged += OnSlimeHpChange;
    }

   public void Init(Slime slime)
    {
        NameText.text = slime.eduProperties.name;
        maxHP = slime.battleProperties.maxHP;
        HPText.text = "HP: " + maxHP.ToString() + "/" + maxHP.ToString();
        HPImage.fillAmount = 1.0f;
        typeText.text = slime.battleProperties.type.ToString();
    }

    private void OnDestroy()
    {
        GameManager.OnSlimeHpChanged -= OnSlimeHpChange;
    }

    void OnSlimeHpChange(int hp,bool _isPlayer)
    {
        if(_isPlayer != isPlayer) return;
        
        HPText.text = "HP: " + hp + "/" + maxHP;
        HPImage.fillAmount = hp / (float)maxHP;
    }
    
}
