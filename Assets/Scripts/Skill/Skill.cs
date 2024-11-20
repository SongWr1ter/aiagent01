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
    public int count;
    private string criticalOrDoge;
    private bool doge;
    private bool critical;

    public Skill(string name, string description, SkillType skillType, SlimeType type, float number, int count,
        bool buffTargetedSelf = true)
    {
        this.name = name;
        this.description = description;
        this.skillType = skillType;
        this.type = type;
        this.number = number;
        this.count = count;
        this.buffTargetedSelf = buffTargetedSelf;
    }
    /// <summary>
    /// ??????
    /// </summary>
    /// <param name="user"></param>
    /// <param name="obj"></param>
    /// <returns>obj???????</returns>
    public virtual bool Discharge(Slime user, Slime obj)
    {
        --count;
        // if (--count < 0)
        // {
        //     Debug.Log($"{user.eduProperties.name}?????{name},??????????");
        // }
        // else
        
        //Debug.Log($"{user.eduProperties.name}对{obj.eduProperties.name}释放了{name}");
        
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
        criticalOrDoge = "";
        doge = false;
        critical = false;
        float damage = GameplayManager.GetDamage(this,user,target);
        if (damage == -1.0f)
        {
            //被闪避
            doge = true;
            damage = 0;
            criticalOrDoge = target.eduProperties.name + "闪开了" + user.eduProperties.name + "的"+name+"!";
        }else if (damage < 0)
        {
            //暴击
            critical = true;
            damage *= -1.0f;
            criticalOrDoge = user.eduProperties.name + "打出了暴击!";
        }
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

    public string GetDogeOrCrtic()
    {
        return criticalOrDoge;
    }

    public bool GetCriticalOrDoge(int i)
    {
        if (i == 0) return critical;
        else return doge;
    }
}