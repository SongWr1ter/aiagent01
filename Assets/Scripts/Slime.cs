using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleProperties
{
    public SlimeType type;
    public int HP;
    public int maxHP;
    public float Attack;
    public float Defence;
    public float Luck;
    public float Speed;
}
[Serializable]
public class EduProperties
{
    public string name;
    public Personality personality;
    public float Intimacy;
    public float Grown;
    public bool Awaken;
    public bool isPlayer;
}

[Serializable]
public class DesireProperties
{
    public float attackDesire;
    public float defendDesire;
    public float skillDesire;
    public float noobDesire;
    public float strategyDesire;
}

public class Slime : MonoBehaviour
{
    public BattleProperties battleProperties;
    public EduProperties eduProperties;
    public DesireProperties desireProperties;
    public List<Skill> skills = new List<Skill>();
    
    /// <summary>
    /// 接受伤害并判定是否死亡
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>true=Slime dead</returns>
    public bool RecvDamage(float damage)
    {
        int damageInt = Mathf.RoundToInt(damage);
        battleProperties.HP -= damageInt;
        GameManager.OnSlimeHpChanged?.Invoke(battleProperties.HP,eduProperties.isPlayer);
        if (battleProperties.HP <= 0)
        {
            battleProperties.HP = 0;
            return true;
        }
        return false;
    }

    public string GetSkillDescription()
    {
        string description = "";
        if (skills.Count <= 0)
        {
            description = "No skills!";
        }
        else
        {
            for (int i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                string desp = "技能" + i + ":" + skill.description + ",";
                description += desp;
            }
        }
        
        return description;
    }

    public void LearnSkill(Skill skill)
    {
        skills.Add(skill);
    }
    
    
    
}
