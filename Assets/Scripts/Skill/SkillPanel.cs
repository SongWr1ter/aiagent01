using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SkillPanel : MonoBehaviour
{
    private List<Skill> skills = new List<Skill>();
    private int skillIndex = 0;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescriptionText;
    [SerializeField] private TMP_Text skillTypeText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text skillNumberText;
    [SerializeField] private TMP_Text skillCountText;
    private Vector3 startPosition;
    public float height;
    private bool state = true;
    private void Start()
    {
        startPosition = transform.position;
        toggle();
        GetComponent<CanvasGroup>().alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PanelInit(GameManager.Instance.playerSlime.skills);
            //PanelInit(Tskills);
            toggle();
        }
    }

    void toggle()
    {
        if (state)
        {
            transform.DOMoveY(startPosition.y-height, .75f).SetEase(Ease.OutBounce);
        }
        else
        {
            transform.DOMoveY(startPosition.y, .75f).SetEase(Ease.OutBounce);
            GetComponent<CanvasGroup>().alpha =1f;
        }
        state = !state;
    }

    public void PanelInit(List<Skill> skills)
    {
        this.skills = skills;
        SetText(0);
    }

    public void Switch2Player()
    {
        PanelInit(GameManager.Instance.playerSlime.skills);
    }
    
    public void Switch2Enemy()
    {
        PanelInit(GameManager.Instance.enemySlime.skills);
    }

    public void forward()
    {
        if (++skillIndex >= skills.Count)
        {
            skillIndex = 0;
        }
        SetText(skillIndex);
    }

    public void backward()
    {
        if (--skillIndex < 0)
        {
            skillIndex = skills.Count - 1;
        }
        SetText(skillIndex);
    }

void SetText(int index)
    {
        if (skills.Count <= index)
        {
             return;
        }
        var skill = skills[index];
        string type;
        string skillType;
        string number;
        switch (skill.skillType)
        {
            case SkillType.ATTACK:
                skillType = "攻击型";
                break;
            default:
                skillType = "增益型";
                break;
        }

        switch (skill.type)
        {
            case SlimeType.Fire:
                type = "火";
                break;
            default:
                type = "无";
                break;
            case SlimeType.Lightning:
                type = "雷";
                break;
            case SlimeType.Water:
                type = "水";
                break;
            case SlimeType.Wood:
                type = "木";
                break;
        }

        number = (100 * skill.number) + "%";
        
        skillNameText.text = "技能名称:"+ skill.name;
        skillDescriptionText.text = "技能描述:"+skill.description;
        skillTypeText.text = "技能类型:"+skillType;
        typeText.text = "技能属性:"+type;
        skillNumberText.text = "技能数值:"+number;
        skillCountText.text = "使用次数"+skill.count;
    }
}
