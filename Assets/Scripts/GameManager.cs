using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using GlmUnity;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class GameManager : SingleTon<GameManager>
{
    public enum GameState
    {
        Start,
        Intervuring,
        GameOver
    }

    class turnEventRecord
    {
        public Slime user;
        public Slime target;
        public int skill_id;
        public Action action;
        public float number;
        public SlimeAnimator.SlimeAnimation userAnimation;
        public SlimeAnimator.SlimeAnimation targetAnimation;
    }
    public const string AttackPrompt = "attack";
    public const string DefendPrompt = "defend";
    public const string SkillPrompt = "skill";
    const string pattern = @"skill(\d+)";
    public const string NoobPrompt = "noob";
    public const string StrategyPrompt = "strategy";
    public static Action<int, bool> OnSlimeHpChanged;
    private GameState gameState;
    private bool mutex = false;
    private bool canContinue = true;
    [SerializeField]private string criticalOrDoge;//描述一回合中谁暴击了谁闪避了
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    [HideInInspector] public Slime playerSlime;
    [HideInInspector] public Slime enemySlime;
    public Transform playerSpawn;
    public Transform enemySpawn;
    public StatePanelItem playerPanel;
    public StatePanelItem enemyPanel;
    public TextPanel textPanel;
    private GLMLoading glmLoading;
    private GameOverPanel gameOverPanel;
    private InterveneButton interveneButton;
    public Button continueButton;
    public SlimeSetSO slimeSet;
    [HideInInspector]public static List<string> api_keys = new List<string>();
    [HideInInspector]public static List<string> secrect_keys = new List<string>();
    public static List<string> keys = new List<string>
    {
        "f2090ce4c5f65389b1177a024bae420c.OFvk0T14U0eGgYv6",
        "7f5b20f796c1588342dc17f9cfb621da.spD6aRgXLCHAFsiO"
    };
    private List<turnEventRecord> turnEventRecords = new List<turnEventRecord>();
    public Sprite gameOverSprite;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0;i<keys.Count;i++)
        {
            var k = keys[i];
            var _apiKey = k.Split('.')[0];
            var _secretKey = k.Split(".")[1];
            api_keys.Add(_apiKey);
            secrect_keys.Add(_secretKey);
        }

        playerPrefab = slimeSet.GetPlayerSlime();
        enemyPrefab = slimeSet.GetRandomSlime();
    }

    private void Start()
    {
        glmLoading = GameObject.FindObjectOfType<GLMLoading>();
        interveneButton = GameObject.FindObjectOfType<InterveneButton>();
        gameOverPanel = GameObject.FindObjectOfType<GameOverPanel>();
        continueButton.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {
        continueButton.onClick.RemoveListener(OnButtonClick);
    }

    public void Setup()
    {
        gameState = GameState.Start;
        
        StartCoroutine(SetupBattle());
    }
    bool playerFirst;
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
        
        GLMStart();
        //开启绝命乱斗的循环!
        int i = 0;
        while (gameState != GameState.GameOver)
        {
            turnEventRecords.Clear();
            //决定谁先手
            playerFirst = (playerSlime.battleProperties.Speed >= enemySlime.battleProperties.Speed);
            criticalOrDoge = "";
            SetMutex(true);
            canContinue = false;
            GlmTakeTurn(playerFirst);
            while (mutex)
            {
                yield return new WaitForSeconds(1f);
            }
            print("No." + (++i) +"Turn Over ready2next round");
            TurnOverEventHandler();
            while (!canContinue)
            {
                if (gameState == GameState.GameOver)
                {
                    continueButton.GetComponent<Image>().sprite = gameOverSprite;
                    //玩家死亡，游戏失败画面
                    if(playerSlime.battleProperties.HP <= 0)
                    {
                        BattleDescription("\n史莱姆" + playerSlime.eduProperties.name + "倒下了！游戏结束",true);
                    }
                    //敌人死亡，游戏继续
                    else
                    {
                        BattleDescription("\n史莱姆" + enemySlime.eduProperties.name + "倒下了！"+playerSlime.eduProperties.name+"取得了胜利！",true);
                    }

                    canContinue = true;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        //游戏结束
        if (gameState == GameState.GameOver)
        {
            
        }
    }
    #region UI功能
    private void OnButtonClick()
    {
        if (gameState == GameState.GameOver)
        {
            return;
        }
        if (mutex == false)
        {
            canContinue = true;
            textPanel.StartTyping("史莱姆思考中...");
        }
    }

    public void PlayerInterveneSend(string text)
    {
        if (mutex)
        {
            Debug.LogError("干预时间不对");
            return;
        }
        SendData prompt = new SendData
        {
            role = "user",
            content = "现在玩家作为"+playerSlime.eduProperties.name+"的训练师，在这回合开始前对其说了下面这段话\n\""+text+"\""
        };
        chatHistory.Add(prompt);
    }
    #endregion

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
    void AttackChoice(Slime attacker,Slime defender)
    {
        float damage = GameplayManager.GetDamage(attacker,defender);
        bool doge = false;
        if (damage == -1)
        {
            //被闪避
            damage = 0;
            doge = true;
            criticalOrDoge += defender.eduProperties.name + "闪开了" + attacker.eduProperties.name + "的攻击!";
        }else if (damage < 0)
        {
            //暴击
            damage *= -1.0f;
            criticalOrDoge += attacker.eduProperties.name + "打出了暴击!";
        }
        
        turnEventRecords.Add(new turnEventRecord
        {
            user = attacker,
            target = defender,
            action = () =>
            {
                bool res = defender.RecvDamage(damage);
                if (doge) defender.DogeReact();
                if (res == true)
                {
                    //defender has dead
                    gameState = GameState.GameOver;
                }
            },
            number = damage,
            userAnimation = SlimeAnimator.SlimeAnimation.jump_attack,
            targetAnimation = doge?SlimeAnimator.SlimeAnimation.idle:SlimeAnimator.SlimeAnimation.hurt
        });
    }

    void DefendChoice(Slime slime)
    {//回合结束后要删除这个效果
        float bonus = slime.battleProperties.Defence * 0.5f;
        
        turnEventRecords.Add(new turnEventRecord
        {
            user = slime,
            target = null,
            action = () =>
            {
                slime.DefendReact();
                slime.battleProperties.Defence += bonus;
                AddTurnOverEvent(new TurnOverEvent
                {
                    duration = 1,
                    eventType = TurnOverEvent.EventType.DefendOver,
                    obj = slime,
                    x = bonus
                });
            },
            number = bonus
        });
    }

    void SkillChoice(Slime user, Slime obj, int skillID)
    {
        
        
        turnEventRecords.Add(new turnEventRecord
        {
            user = user,
            target = obj,
            action = () =>
            {
                if (user.skills[skillID].Discharge(user, obj))
                {
                    gameState = GameState.GameOver;
                }

                criticalOrDoge += user.skills[skillID].GetDogeOrCrtic();
                if (user.skills[skillID].GetCriticalOrDoge(1))
                {
                    obj.DogeReact();
                }
            },
            skill_id = skillID,
            userAnimation = SlimeAnimator.SlimeAnimation.jump_attack,
            targetAnimation = SlimeAnimator.SlimeAnimation.hurt
        });
    }

    void NoobChoice(Slime user)
    {
        turnEventRecords.Add(new turnEventRecord
        {
            user = user,
            target = null,
            action = () =>
            {
                user.NoobReact();
            },
            skill_id = 0,
            userAnimation = SlimeAnimator.SlimeAnimation.idle,
        });
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
        //就是在遍历时，不要改变正在遍历的集合即可，您可以先遍历完在对其进行操作。
        // foreach (TurnOverEvent e in turnOverEvents)
        // {
        //     --e.duration;
        //     switch (e.eventType)
        //     {
        //         case TurnOverEvent.EventType.DefendOver:
        //             e.obj.battleProperties.Defence -= e.x;
        //             break;
        //         default:
        //             break;
        //     }
        //
        //     if (e.duration <= 0)
        //     {
        //         turnOverEvents.Remove(e);
        //     }
        // }
        for (int i = 0; i < turnOverEvents.Count; i++)
        {
            var e = turnOverEvents[i];
            --e.duration;
            switch (e.eventType)
            {
                case TurnOverEvent.EventType.DefendOver:
                    e.obj.battleProperties.Defence -= e.x;
                    break;
                default:
                    break;
            }

            if (e.duration <= 0)
            {
                turnOverEvents.Remove(e);
            }
        }
    }

    #endregion
    
    #region 大模型
    [SerializeField, Tooltip("玩家和GLM的历史对话记录以List<SendDat>格式保存")]
    private List<SendData> chatHistory = new List<SendData>();
    [SerializeField, Tooltip("玩家和GLM的历史对话记录以List<SendDat>格式保存")]
    private List<SendData> chatHistoryV2 = new List<SendData>();
    // 定义一个类来匹配JSON的结构
    [System.Serializable]
    public class LogData
    {
        public string predict_result1;
        public string predict_result2;
        // public string log;
    }
    private void GLMStart()
    {
        #region battle

        chatHistory.Clear();
        // 创建system prompt，让GLM进行角色扮演。System prompt不是强制要求的
        SendData systemPrompt = new SendData("system",
            "有两个史莱姆正在进行回合制战斗，史莱姆能做出的行动有四种：攻击，防御，施放法术以及发呆。每个史莱姆" +
            "的属性是水火木雷四个属性中的一个，四个属性相互克制，如水克火，火克木，木克雷，雷克水。攻击行动不会触发克制。" +
            "在每个回合开始前，我会告诉你两个史莱姆的状态，并告诉你第一个行动的史莱姆是谁。" +
            "现在每当我说出第一个行动的史莱姆之后，告诉我每个史莱姆在这个回合进行的行动，只预测一个结果" +
            "预测结果的取值从\"attack\"、\"defend\"、\"skillx\"、\"noob\"中选择" +
            "最终以json格式输出两个变量:\"predict_result1\":先手史莱姆的行动、\"predict_result2\":后史莱姆的行动" +
            "。\"skillx\"里的x是技能编号.");

// 将system prompt作为第一条对话记录加入chatHistory
        chatHistory.Add(systemPrompt);
        

        #endregion

        #region broadcast

        
        
        chatHistoryV2.Clear();
        // 创建system prompt，让GLM进行角色扮演。System prompt不是强制要求的
        SendData systemPromptv2 = new SendData("system",
            "有两个史莱姆正在进行回合制战斗，史莱姆能做出的行动有四种：攻击，防御，施放法术以及发呆。每个史莱姆" +
            "的属性是水火木雷四个属性中的一个，四个属性相互克制，如水克火，火克木，木克雷，雷克水。攻击行动不会触发克制。" +
            "现在我会依次告诉你每回合两个史莱姆做出的行动以及行动的结果，请你以一位活泼可爱的比赛解说的身份，对战斗情况进行描述" +
            "输出结果以\"Amy:\"开头");
        // 将system prompt作为第一条对话记录加入chatHistory
        chatHistoryV2.Add(systemPromptv2);
        #endregion

       
    }

    void GlmTakeTurn(bool _playerFirst)
    {
        string input = "这一回合";
        string x = _playerFirst == true ? playerSlime.eduProperties.name: enemySlime.eduProperties.name;
        string y = _playerFirst == false ? playerSlime.eduProperties.name: enemySlime.eduProperties.name;
        string playerInput = $"史莱姆{playerSlime.eduProperties.name}的生命值有{playerSlime.battleProperties.HP},防御值{playerSlime.battleProperties.Defence},攻击值{playerSlime.battleProperties.Attack}," +
                             $"暴击率{playerSlime.battleProperties.Luck / 200.0f}%,闪避率{playerSlime.battleProperties.Speed / 200.0f}%" +
                             $"属性为{playerSlime.battleProperties.type},技能描述:{playerSlime.GetSkillDescription()},性格为{playerSlime.eduProperties.personality}\n";
        string enemyInput = $"史莱姆{enemySlime.eduProperties.name}的生命值有{enemySlime.battleProperties.HP},防御值{enemySlime.battleProperties.Defence},攻击值{enemySlime.battleProperties.Attack}" +
                            $"暴击率{enemySlime.battleProperties.Luck / 200.0f}%,闪避率{enemySlime.battleProperties.Speed / 200.0f}%" +
                            $"属性为{enemySlime.battleProperties.type},技能描述:{enemySlime.GetSkillDescription()},性格为{enemySlime.eduProperties.personality}";
        
        var playerMessage = new SendData()
        {
            role = "user", 
            content = input + x + "先手" + "\n两个史莱姆描述如下"+ playerInput + enemyInput + "" +
                      "以json格式输出你的结果，包含两个变量:\"predict_result1\":先手史莱姆"+x+"的行动、\"predict_result2\":后手史莱姆"+y+"的行动"
        };
        
        chatHistory.Add(playerMessage);
        
        Action(_playerFirst);
    }
    
    private async void Action(bool _playerFirst)
    {
        #region 发送背景条件，让GLM返回战斗流程
        // 读取玩家输入并创建用户信息
        

        // 使用GLMHandler.GenerateGLMResponse生成GLM回复，设置tmeperature=0.8
        // 注意需要使用await关键词
        SendData respone_origin = await GlmHandler.GenerateGlmResponse(chatHistory, 0.8f);
        
        //print(respone_origin.content);
        #endregion
       
        #region 处理GLM发来的json数据
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
        chatHistory.Add(respone_origin);
        StartCoroutine(ActionShow(data, _playerFirst));
    }

    private async void BattleDescription(string bd,bool flag = false)
    {
        var msg = new SendData()
            {
                role = "user", 
                content = "这一回合的情况:\n" +
                          bd + "\n输出结果以\"Amy\"开头\"" 
            };
        chatHistoryV2.Add(msg);
        SendData respone_origin = await GlmHandler.GenerateGlmResponseV2(chatHistoryV2, 0.8f);
        chatHistoryV2.Add(respone_origin);
        textPanel.StartTyping(respone_origin.content);
        SetMutex(false);//本回合回合结束，主Corotine恢复运行
        //continueButton.gameObject.SetActive(true);
        if (flag)
        {
            //Game Over Already
            gameOverPanel.OnGameOver(playerSlime.battleProperties.HP > 0);
        }
    }
    
    #endregion

    #region 演出效果

    IEnumerator ActionShow(LogData data,bool _playerFirst)
    {
        Slime first, second;
        if (_playerFirst)
        {
            first = playerSlime;
            second = enemySlime;
        }
        else
        {
            first = enemySlime;
            second = playerSlime;
        }
        SlimeAction(first,second,data.predict_result1);//1号演出
        SlimeAction(second,first,data.predict_result2);//2号演出
        
        string bd =
            $"{first.eduProperties.name}做出了{ActionTrans(data.predict_result1,first)}，{second.eduProperties.name}做出了{ActionTrans(data.predict_result2,second)}。\n" +
            criticalOrDoge;
        BattleDescription(bd);//解说
        yield return new WaitForSeconds(1f);
        StartCoroutine(ActionAnimationMain());
        //SetMutex(false);//本回合回合结束，主Corotine恢复运行
        //print($"{first.eduProperties.name}先手。选择{data.predict_result1},{second.eduProperties.name}选择{data.predict_result2}");
       
    }

    IEnumerator ActionAnimationMain()
    {
        for (int i = 0; i < 2; i++)
        {
            var record = turnEventRecords[i];
            if(record.action!=null) record.action();
            record.user.SetAnimatorState(record.userAnimation);
            if(record.target != null) record.target.SetAnimatorState(record.targetAnimation);
            yield return new WaitForSeconds(1.0f);
            record.user.SetAnimatorState(SlimeAnimator.SlimeAnimation.idle);
            if(record.target != null) record.target.SetAnimatorState(SlimeAnimator.SlimeAnimation.idle);
            yield return new WaitForSeconds(1f);
            if (gameState == GameState.GameOver)
            {
                //玩家死亡，游戏失败画面
                if(playerSlime.battleProperties.HP <= 0)
                {
                    playerSlime.SetAnimatorState(SlimeAnimator.SlimeAnimation.die);
                }
                //敌人死亡，游戏继续
                else if(enemySlime.battleProperties.HP <= 0)
                {
                    enemySlime.SetAnimatorState(SlimeAnimator.SlimeAnimation.die);
                }
                yield break;
            }
            
        }
        
    }

    #endregion
    

    private string ActionTrans(string action,Slime slime)
    {
        Regex regex = new Regex(pattern);
        var resl = regex.Match(action);
        if (resl.Success)
        {
            string number = resl.Groups[1].Value;
            return "释放技能:"+slime.skills[int.Parse(number)].name;
        }
        switch (action)
        {
            case AttackPrompt:
                return "攻击";
            case DefendPrompt:
                return "防御";
            case NoobPrompt:
                return "发呆";
            case StrategyPrompt:
                return "策略";
            default:
                Debug.LogError("Unknown action prompt");
                return "ERROR";
        }
    }

    private void SetMutex(bool value)
    {
        mutex = value;
        glmLoading.SetAnimation(value);
        interveneButton.SetLock(value);
        continueButton.gameObject.SetActive(!value);
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
