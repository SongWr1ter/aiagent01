using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class GameManager : SingleTon<GameManager>
{
    public enum GameState
    {
        Start,
        Intervuring,
        PlayerTurn,
        EnemyTurn,
        GameOver
    }
    const string AttackPrompt = "attack";
    const string DefendPrompt = "defend";
    const string SkillPrompt = "skill";
    const string NoobPrompt = "noob";
    const string StrategyPrompt = "strategy";
    private GameState gameState;
    
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    [HideInInspector] public Slime playerSlime;
    [HideInInspector] public Slime enemySlime;
    private bool playerFirst;
    public Transform playerSpawn;
    public void Setup()
    {
        gameState = GameState.Start;

        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        //选手就为
        GameObject player = Instantiate(playerPrefab);
        player.transform.position = playerSpawn.position;
        playerSlime = player.GetComponent<Slime>();
        GameObject enemy = Instantiate(enemyPrefab);
        enemy.transform.position = playerSpawn.position;
        enemySlime = enemy.GetComponent<Slime>();
        
        yield return new WaitForSeconds(2f);
        
        //决定谁先手
        playerFirst = (playerSlime.battleProperties.Speed >= enemySlime.battleProperties.Speed);
        
        //开启绝命乱斗的循环!
        while (gameState != GameState.GameOver)
        {
            TakeTurns();
        }

        //游戏结束
        //玩家死亡，游戏失败画面
        if(playerSlime.battleProperties.HP <= 0)
        {

        }
        //敌人死亡，游戏继续
        else
        {

        }
    }

    void TakeTurns()
    {
        if (playerFirst)
        {
            gameState = GameState.PlayerTurn;
            playerSlime.Action();
            if(gameState == GameState.GameOver) return;
            gameState = GameState.EnemyTurn;
            enemySlime.Action();
        }
        else
        {
            gameState = GameState.EnemyTurn;
            enemySlime.Action();
            if(gameState == GameState.GameOver) return;
            gameState = GameState.PlayerTurn;
            playerSlime.Action();
        }
        
    }
    /// <summary>
    /// 实际执行史莱姆的行动。
    /// type有五类attack,defend,skillX,noob,strategy
    /// 其中，为了知道skill所释放的是什么技能，需要在后面进行补充技能id（是史莱姆的技能id不是技能本身的id）
    /// </summary>
    /// <param name="type"></param>
    void SlimeAction(Slime user,Slime obj,string type)
    {
        //先检验技能

        // 正则表达式，匹配 "skill" 后跟一个或多个数字
        string pattern = @"skill(\d+)";
        // 创建正则表达式对象
        Regex regex = new Regex(pattern);
        var resl = regex.Match(type);
        if (resl.Success)
        {
            string number = resl.Groups[1].Value;
            SkillChoice(user, obj, int.Parse(number));
        }
        //再检验其他类型

        else
        {
            switch (type)
            {
                case AttackPrompt:
                    AttackChoice(user, obj);
                    break;
                case DefendPrompt:
                    DefendChoice(user);
                    break;
                case NoobPrompt:
                    NoobChoice(user);
                    break;
                case StrategyPrompt:
                    StrategyChoice(user);
                    break;
                default:
                    Debug.LogError("Unknown action prompt");
                    break;
            }
        }
        
    }

    #region 史莱姆的选择
    void AttackChoice(Slime attacker,Slime defender)
    {
        float damage = GameplayManager.GetDamage(attacker,defender);
        bool res = defender.RecvDamage(damage);
        if (res == true)
        {
            //defender has dead
            gameState = GameState.GameOver;
        }
    }

    void DefendChoice(Slime slime)
    {//回合结束后要删除这个效果
        float bonus = slime.battleProperties.Defence * 0.5f;
        slime.battleProperties.Defence += bonus;
        AddTurnOverEvent(new TurnOverEvent
        {
            duration = 1,
            eventType = TurnOverEvent.EventType.DefendOver,
            obj = slime,
            x = bonus
        });
    }

    void SkillChoice(Slime user, Slime obj, int skillID)
    {
       bool result = user.skills[skillID].Discharge(user, obj);
    }

    void NoobChoice(Slime user)
    {

    }

    void StrategyChoice(Slime user)
    {
        //read from GPT
    }
    #endregion
    

    #region 回合结束事件处理,目前只有一回合的效果

    public class TurnOverEvent
    {
        public Slime obj;
        public EventType eventType;
        public int duration = 1;
        public float x;//对于防御而言，它是增加的防御值
        public enum EventType
        {
            None,
            DefendOver
        }
    }
    public List<TurnOverEvent> turnOverEvents = new List<TurnOverEvent>();

    public void AddTurnOverEvent(TurnOverEvent e)
    {
        turnOverEvents.Add(e);
    }
    public void TurnOverEventHandler()
    {
        foreach (TurnOverEvent e in turnOverEvents)
        {
            --e.duration;
            switch (e.eventType)
            {
                case TurnOverEvent.EventType.DefendOver:
                    e.obj.battleProperties.Defence -= e.x;
                    break;
                default:
                    break;
            }

            if (e.duration == 0)
            {
                turnOverEvents.Remove(e);
            }
        }
        
    }

    #endregion
    
}
