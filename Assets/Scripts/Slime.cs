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
    public Personality personality;
    public float Intimacy;
    public float Grown;
}

public class Slime : MonoBehaviour
{
    public BattleProperties battleProperties;
    public EduProperties eduProperties;
    public List<string> skills = new List<string>();
    
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

    public void Action()
    {
        
    }
}
