using UnityEngine;

[System.Serializable]
public enum SkillType
{
    ATTACK,
    BUFF
}
public class Skill
{
    public string name;
    public SkillType skillType; 
    public SlimeType type;//水火木电
    public float number;//攻击型技能是伤害，增益型技能是百分比
    public int count;//使用次数
    /// <summary>
    /// 释放技能
    /// </summary>
    /// <param name="user"></param>
    /// <param name="obj"></param>
    /// <returns>obj是否死亡</returns>
    public virtual bool Discharge(Slime user, Slime obj)
    {
        if (--number < 0)
        {
            Debug.Log($"{user.eduProperties.name}释放了{name},但没有释放成功");
        }
        else
        {
            Debug.Log($"{user.eduProperties.name}对{obj.eduProperties.name}释放了{name}");
        }
        return false;
        
    }
}