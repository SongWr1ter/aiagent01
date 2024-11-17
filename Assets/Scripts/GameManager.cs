using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using GlmUnity;


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
    public const string AttackPrompt = "attack";
    public const string DefendPrompt = "defend";
    public const string SkillPrompt = "skill";
    const string pattern = @"skill(\d+)";
    public const string NoobPrompt = "noob";
    public const string StrategyPrompt = "strategy";
    public static Action<int, bool> OnSlimeHpChanged;
    private GameState gameState;
    
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    [HideInInspector] public Slime playerSlime;
    [HideInInspector] public Slime enemySlime;
    private bool playerFirst;
    public Transform playerSpawn;
    public Transform enemySpawn;
    public StatePanelItem playerPanel;
    public StatePanelItem enemyPanel;

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
        playerPanel.Init(playerSlime);
        GameObject enemy = Instantiate(enemyPrefab);
        enemy.transform.position = enemySpawn.position;
        enemySlime = enemy.GetComponent<Slime>();
        enemyPanel.Init(enemySlime);
        
        yield return new WaitForSeconds(1f);
        
        //决定谁先手
        playerFirst = (playerSlime.battleProperties.Speed >= enemySlime.battleProperties.Speed);
        
        //开启绝命乱斗的循环!
        while (gameState != GameState.GameOver)
        {
            if (playerFirst)
            {
                gameState = GameState.PlayerTurn;
                Action(playerSlime,enemySlime);
                yield return new WaitForSeconds(2f);
                if(gameState == GameState.GameOver) yield break;
                gameState = GameState.EnemyTurn;
                Action(enemySlime,playerSlime);
                yield return new WaitForSeconds(2f);
            }
            else
            {
                gameState = GameState.EnemyTurn;
                Action(enemySlime,playerSlime);
                yield return new WaitForSeconds(2f);
                if(gameState == GameState.GameOver) yield break;
                gameState = GameState.PlayerTurn;
                Action(playerSlime,enemySlime);
                yield return new WaitForSeconds(2f);
            }
            
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

    public void debugattack(Slime attacker,Slime defender)
    {
        AttackChoice(attacker, defender);
    }
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
    
    [SerializeField, Tooltip("玩家和GLM的历史对话记录以List<SendDat>格式保存")]
    private List<SendData> chatHistory = new List<SendData>();
    // 定义一个类来匹配JSON的结构
    [System.Serializable]
    public class LogData
    {
        public string predict_result;
        public string log;
    }
    private void Start()
    {
        // 创建system prompt，让GLM进行角色扮演。System prompt不是强制要求的
        SendData systemPrompt = new SendData("system",
            "有两个史莱姆正在进行回合制战斗，史莱姆能做出的行动有四种：攻击，防御，施放法术以及发呆。每个史莱姆" +
            "的属性是水火木雷四个属性中的一个，四个属性相互克制，如水克火，火克木，木克雷，雷克水。根据下面我对两个" +
            "史莱姆的描述，预测第一个史莱姆在下一个回合进行的行动，只预测一个结果,并以一位活泼可爱的比赛解说身份对史莱姆的行动进行现场播报" +
            "最终以json格式输出两个变量:\"predict_result\":预测结果和\"log\":解说记录以amy开头。" +
            "。施放法术时要在后面加上技能编号.");


        // 将system prompt作为第一条对话记录加入chatHistory
        chatHistory.Add(systemPrompt);
    }
    public async void Action(Slime user, Slime target)
    {
        string output;
        #region 发送背景条件，让GLM返回战斗流程
        // 读取玩家输入并创建用户信息
        string playerInput = $"第一个史莱姆{user.eduProperties.name}的生命值有{user.battleProperties.HP},防御值{user.battleProperties.Defence},攻击值{user.battleProperties.Attack}" +
                             $"属性为${user.battleProperties.type},技能描述:{user.GetSkillDescription()}";
        string enemyInput = $"第二个史莱姆{target.eduProperties.name}的生命值有{target.battleProperties.HP},防御值{target.battleProperties.Defence},攻击值{target.battleProperties.Attack}" +
                             $"属性为${target.battleProperties.type},技能描述:{target.GetSkillDescription()}";
       
        var playerMessage = new SendData()
        {
            role = "user", 
            content = playerInput + enemyInput
        };
        
        chatHistory.Add(playerMessage);

        // 使用GLMHandler.GenerateGLMResponse生成GLM回复，设置tmeperature=0.8
        // 注意需要使用await关键词
        SendData respone_origin = await GlmHandler.GenerateGlmResponse(chatHistory, 0.8f);
        print(respone_origin.content);
        #endregion
       
        #region 让处理GLM自己发来的数据，返回json
        // 将JSON字符串解析为LogData对象
        
        string extractedString = "";
        string start = "```json\n";
        string end = "```";
 
        int startIndex = respone_origin.content.IndexOf(start) + start.Length;
        int endIndex = respone_origin.content.IndexOf(end, startIndex);

        if (startIndex >= 0 && endIndex >= 0)
        {
            extractedString = respone_origin.content.Substring(startIndex, endIndex - startIndex);
        }

        LogData data = JsonUtility.FromJson<LogData>(extractedString);
        
        #endregion
        print(data.log +"]\n["+ data.predict_result);
        //SlimeAction(user,target,output);
    }
}
/*
 * // 使用GlmFunctionTool类创建一个函数调用工具，并加入函数需要返回的参数
        GlmFunctionTool functionTool = new GlmFunctionTool("battle_flow", "提取回合制行动信息");
        // 这才是观察向量
        functionTool.AddProperty("action_type", "string", "第一个史莱姆行动类型",
            true, new List<string>
            {
                GameManager.AttackPrompt,GameManager.DefendPrompt
                ,GameManager.NoobPrompt,GameManager.SkillPrompt
            });
        functionTool.AddProperty("skill_id", "int", "如果选择释放技能，那么提取技能编号",
            false);
        
        // 发送GLM函数请求，注意需要将functionTool放在一个List<GlmTool>工具列表里
        SendData response = await GlmHandler.GenerateGlmResponse(chatHistory, 0.6f, new List<GlmTool> { functionTool });

        // GLM会根据user的输入内容和函数是否相关，决定是使用函数工具还是生成普通对话
        // 如果response.tool_calls != null，则代表GLM使用了函数工具
        if (response.tool_calls != null)
        {
            // 使用response.tool_calls[0].arguments_dict来获取输出参数
            Dictionary<string, string> functionOutput = response.tool_calls[0].arguments_dict;
            output = functionOutput.ContainsKey("action_type") ? functionOutput["action_type"] : "";
            int id = functionOutput.ContainsKey("skill_id") ? int.Parse(functionOutput["skill_id"]) : 0;
            if (output == GameManager.SkillPrompt)
            {
                output += id.ToString();
            }
        }
        // 如果user的输入与函数不相关，GLM会执行普通的对话，不调用函数。
        else
        {
            output = "";
            //Debug.Log(response.content);
        }
 */
