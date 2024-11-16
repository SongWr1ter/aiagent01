using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private GameState gameState;
    
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    [HideInInspector] public Slime playerSlime;
    [HideInInspector] public Slime enemySlime;
    private bool playerFirst;
    public Transform playerSpawn;
    private void Setup()
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

    #region 史莱姆的选择
    public void AttackChoice(Slime attacker,Slime defender)
    {
        float damage = GameplayManager.GetDamage(attacker,defender);
        bool res = defender.RecvDamage(damage);
        if (res == true)
        {
            //defender has dead
        }
    }

    public void DefendChoice(Slime slime)
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

    public void SkillChoice(Slime user, Slime obj, string skill)
    {
        
    }

    public void StrategyChoice()
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
