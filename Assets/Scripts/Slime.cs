using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleProperties
{
    public SlimeType type;
    public int HP;
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
    private DesireProperties desireProperties;
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
        if (battleProperties.HP <= 0)
        {
            battleProperties.HP = 0;
            return true;
        }
        return false;
    }

    public void LearnSkill(Skill skill)
    {
        skills.Add(skill);
    }

    public string Action()
    {
        //向GLM发送信息
        string sendStr = "hello";
        //从GLM接受信息
        string recvStr = "attack";
        //根据信息执行行动
        return recvStr;
    }
}
