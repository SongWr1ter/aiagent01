using UnityEngine;

[System.Serializable]
public enum SkillType
{
    ATTACK,
    BUFF_DEFENCE,
    BUFF_ATTACK,
    BUFF_HP,
    BUFF_SPEED,
    BUFF_LUCK
}
[System.Serializable]
public class Skill
{
    public string name;
    public string description;
    public SkillType skillType; 
    public SlimeType type;//??????
    [SerializeField]
    public float number;//
    public bool buffTargetedSelf = true;
    /// <summary>
    /// ??????
    /// </summary>
    /// <param name="user"></param>
    /// <param name="obj"></param>
    /// <returns>obj???????</returns>
    public virtual bool Discharge(Slime user, Slime obj)
    {
        // if (--count < 0)
        // {
        //     Debug.Log($"{user.eduProperties.name}?????{name},??????????");
        // }
        // else
        
        Debug.Log($"{user.eduProperties.name}¶Ô{obj.eduProperties.name}ÊÍ·ÅÁË{name}");
        
        bool resl = false;
        switch (skillType)
        {
            case SkillType.ATTACK:
                resl = ATTACKType(user, obj);
                break;
            case SkillType.BUFF_DEFENCE:
                if(buffTargetedSelf)
                    BuffTypeDefence(user);
                else
                    BuffTypeDefence(obj);
                break;
            case SkillType.BUFF_HP:
                if(buffTargetedSelf)
                    BuffTypeHP(user);
                else
                    BuffTypeHP(obj);
                break;
            case SkillType.BUFF_SPEED:
                if(buffTargetedSelf)
                    BuffTypeSpeed(user);
                else
                    BuffTypeSpeed(obj);
                break;
            case SkillType.BUFF_LUCK:
                if(buffTargetedSelf)
                    BuffTypeLuck(user);
                else
                    BuffTypeLuck(obj);
                break;
            case SkillType.BUFF_ATTACK:
                if(buffTargetedSelf)
                    BuffTypeAttack(user);
                else
                    BuffTypeAttack(obj);
                break;
            default:
                break;
        }
        return resl;
        
    }

    bool ATTACKType(Slime user, Slime target)
    {
        float damage = GameplayManager.GetDamage(this,user,target);
        return target.RecvDamage(damage);
    }

    void BuffTypeDefence(Slime target)
    {
        float origin = target.battleProperties.Defence;
        target.battleProperties.Defence += origin * number;
    }
    
    void BuffTypeAttack(Slime target)
    {
        float origin = target.battleProperties.Attack;
        target.battleProperties.Attack += origin * number;
    }
    
    void BuffTypeHP(Slime target)
    {
        int origin = target.battleProperties.HP;
        target.RecvDamage(-origin * number);
    }
    
    void BuffTypeSpeed(Slime target)
    {
        float origin = target.battleProperties.Speed;
        target.battleProperties.Speed += origin * number;
    }
    
    void BuffTypeLuck(Slime target)
    {
        float origin = target.battleProperties.Luck;
        target.battleProperties.Luck += origin * number;
    }
}