
using Unity.Mathematics;
using UnityEngine;

public enum SlimeType
{
    Water,
    Fire,
    Wood,
    Lightning,
    None
}

public enum Personality
{
    Brave,
    Contious,
    Smart,
    Calmly,
    Friendly
}


public static class GameplayManager
{
    const float CRITICAL_COF  = 2.0f;
    //属性克制表
    public static float4x4 CounterMatrix = new float4x4(
           //  水     火    木     电
/*水*/        1.0f, 1.5f, 1.0f, .75f,
/*火*/        .75f, 1.0f, 1.5f, 1.0f,
/*木*/        1.0f, .75f, 1.0f, 1.5f,
/*电*/        1.5f, 1.0f, .75f, 1.0f
    );
    
    //数值计算
    public static float GetCounterConfficient(SlimeType attacker, SlimeType defender)
    {
        if (attacker == SlimeType.None) return 1.0f;
        else
        {
            int attackerInt = (int)attacker;
            int defenderInt = (int)defender;
            return CounterMatrix[defenderInt][attackerInt];
        }
    }

    public static int GetDamage(Slime attacker, Slime defender)
    {
        //是否闪避
        float doge = defender.battleProperties.Speed / 200f;
        float aim = UnityEngine.Random.Range(0, 1f);
        if (aim < doge)
        {
            //全部防出去了啊
            return -1;
        }
        //是否暴击
        float luck = attacker.battleProperties.Luck / 100f;
        float critical  = UnityEngine.Random.Range(0f, 1f);
        bool isCritical = critical < luck;
        //基础攻击值
        float defence = defender.battleProperties.Defence;
        float baseDamage = attacker.battleProperties.Attack - defence;
        baseDamage = baseDamage < 0 ? 0 : baseDamage;
        //是否克制
        // float ressistance = defender.eduProperties.Awaken == true ? .5f : 0.0f;
        // float typeCof = GetCounterConfficient(attacker.battleProperties.type, defender.battleProperties.type);
        // typeCof = typeCof > 1.0f ? (typeCof - ressistance) : typeCof;
        
        float finalDamage = baseDamage * 1.0f * (isCritical ? CRITICAL_COF : 1f);
        Debug.Log($"type: 1.0f,baseDamage: {baseDamage},finalDamage: {finalDamage}");
        return Mathf.RoundToInt(finalDamage);
    }
    
    public static float GetDamage(Skill skill,Slime user, Slime target)
    {
        //是否闪避
        float doge = target.battleProperties.Speed / 200f;
        float aim = UnityEngine.Random.Range(0, 1f);
        if (aim < doge)
        {
            //全部防出去了啊
            return -1;
        }
        //是否暴击
        float luck = user.battleProperties.Luck / 100f;
        float critical  = UnityEngine.Random.Range(0f, 1f);
        bool isCritical = critical < luck;
        //基础攻击值
        float defence = target.battleProperties.Defence;
        float baseDamage = skill.number * user.battleProperties.Attack - defence;
        baseDamage = baseDamage < 0 ? 0 : baseDamage;
        //是否克制
        float ressistance = target.eduProperties.Awaken == true ? .5f : 0.0f;
        float typeCof = GetCounterConfficient(skill.type, target.battleProperties.type);
        typeCof = typeCof > 1.0f ? (typeCof - ressistance) : typeCof;
        
        float finalDamage = baseDamage * typeCof * (isCritical ? CRITICAL_COF : 1f);
        
        return Mathf.RoundToInt(finalDamage);
    }
}
