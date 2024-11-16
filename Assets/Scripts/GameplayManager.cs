
using Unity.Mathematics;

public enum SlimeType
{
    Water,
    Fire,
    Wood,
    Lightning
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
    //属性克制表
    public static float4x4 CounterMatrix = new float4x4(
           //  水     火    木     电
/*水*/        1.0f, 1.5f, 1.0f, .75f,
/*火*/        .75f, 1.0f, 1.5f, 1.0f,
/*木*/        1.0f, .75f, 1.0f, 1.5f,
/*电*/        1.5f, 1.0f, .75f, 1.0f
    );
    
    //数值计算
    public static float GetDamage(Slime AttackerType, Slime DefenderType)
    {
        int attacker= (int)AttackerType.battleProperties.type;
        int defender = (int)DefenderType.battleProperties.type;
        float res = CounterMatrix[attacker][defender];
        
        return res;
    }
}
