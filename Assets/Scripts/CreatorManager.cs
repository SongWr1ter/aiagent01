using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GlmUnity;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using Random = Unity.Mathematics.Random;

public class CrucibleManager : MonoBehaviour
{
    private const int MAX_CAPACITY = 10;
    private List<int> materials = new List<int> { 0, 0, 0, 0, 0 }; // 5种原料的初始数量
    private int currentTotal = 0; // 当前坩埚内的总量
    private string description;
    private string json;

    // UI 元素（需要在 Inspector 中绑定）
    public Text totalText; // 显示总量
    public Button[] addButtons; // 增加按钮
    public Button[] subtractButtons; // 减少按钮
    public TMP_Text[] ingredientSum; // 显示每个原材料的量
    public Button generateButton; // 生成按钮
    public TMP_InputField playerInput; //slime名字
    public TMP_Text informationText;
    public TMP_Text descriptionText;
    [Tooltip("史莱姆模板,从Prefab里面找")] public GameObject slimeTemplete;
    [Tooltip("史莱姆生成的位置")] public Transform generatePos;
    public List<RuntimeAnimatorController> animators = new List<RuntimeAnimatorController>();
    public SlimeSetSO SlimeSetSo;
    public Button proceedButton;
    public SkillList skillList;

    public static readonly string[] colors = new string[]
        { "bej", "black", "blue", "brown", "dark", "gray", "orange", "purple", "red", "pink", "yellow" };

    void Start()
    {
        UpdateUI();
        StartGLM();
    }

    // 增加原料
    public void AddMaterial(int index)
    {
        if (currentTotal < MAX_CAPACITY)
        {
            materials[index]++;
            currentTotal++;
            UpdateUI();
        }
    }

    // 减少原料
    public void SubtractMaterial(int index)
    {
        if (materials[index] > 0)
        {
            materials[index]--;
            currentTotal--;
            UpdateUI();
        }
    }

    // 生成原料向量
    // public void GenerateVector()
    // {
    //     Debug.Log("生成的向量: " + string.Join(", ", materials));
    //     // 在这里添加生成后续逻辑，例如将向量用于其他功能
    // }

    // 更新UI显示
    private void UpdateUI()
    {
        // totalText.text = "当前总量: " + currentTotal + "/" + MAX_CAPACITY;
        informationText.text = json;
        descriptionText.text = description;
        ingredientSum[0].text = materials[0].ToString();
        ingredientSum[1].text = materials[1].ToString();
        ingredientSum[2].text = materials[2].ToString();
        ingredientSum[3].text = materials[3].ToString();
        ingredientSum[4].text = materials[4].ToString();


        // 控制按钮状态
        for (int i = 0; i < materials.Count; i++)
        {
            addButtons[i].interactable = currentTotal < MAX_CAPACITY;
            subtractButtons[i].interactable = materials[i] > 0;
        }

        generateButton.interactable = currentTotal > 0; // 确保至少有一个原料

    }

    [SerializeField] private List<SendData> chatHistory = new List<SendData>();
    private string responseText = "";

    private static string GetColorsWithDot()
    {
        string result = "";
        foreach (var color in colors)
        {
            result += "'" + color + "',";
        }

        return result;
    }

    private void StartGLM()
    {
        // 创建system prompt，让GLM进行角色扮演。System prompt不是强制要求的
        SendData systemPrompt = new SendData()
        {
            role = "system",
            content = "你是史莱姆匠，专门负责根据用户提供的材料创造个性化的史莱姆。你的角色是创意实现者，通过组合不同材料，打造独一无二的史莱姆。\n" +
                      "用户的材料为以下5种的组合，分别为：\n" +
                      "- 火龙血液：制造配方中含有较多这种原料的史莱姆会倾向于更有勇气、更倾向于攻击、也更倾向于火属性；\n" +
                      "- 沼生植物汁液：制造配方中含有较多这种原料的史莱姆会更坚硬，受到更少的伤害，更倾向于使用技能；\n" +
                      "- 陨石粉末：制造配方中含有较多这种原料的史莱姆动作会更灵活迅捷，更倾向于攻击，更容易打出暴击；\n" +
                      "- 圣湖泉水：这种成分会让史莱姆性格更加平静温和，更倾向于防御，运气会更好，也更倾向于水属性；\n" +
                      "- 蜂蜜：这种成分会让史莱姆和训练家更亲密，更容易获得成长\n" +
                      "每一次对话我会按照顺序告诉你这5种材料的含量，还有用户为这只史莱姆取的名字。你需要描述最后生成的这个史莱姆，并且生成一段包含这些关键信息的json代码。json应该包含这些内容：\n" +
                      "Name:史莱姆的名字。\n" +
                      "HP：描述史莱姆的生命力，越高说明史莱姆耐力越高，它的值应该在100~200之间\n" +
                      "type：在'Water','Fire','Wood','Lightning',中选择，代表史莱姆的属性\n " +
                      "Attack：描述史莱姆的攻击力，越高说明史莱姆攻击越高，它的值应该在\n" +
                      "Defence：描述史莱姆的防御力，越高说明史莱姆防御越高，它的值应该在10~50\n" +
                      "Luck：：描述史莱姆的运气，越高说明史莱姆越不容易陷入异常状态，它的值应该在10~50\n" +
                      "Speed：描述史莱姆的速度，越高说明史莱姆身手越灵活，它的值应该在10~50\n" +
                      "personality：描述史莱姆的性格，在'Brave','Contious','Smart','Calmly','Friendly'中选择一种\n" +
                      "Intimacy：描述史莱姆的驯服度，越高说明史莱姆和训练家越亲近，它的值应该在10~50\n" +
                      "Grown：描述史莱姆的成长性，越高说明史莱姆越容易成长，它的值应该在10~50\n" +
                      "Skill：史莱姆学习到的技能名称,技能从" + skillList.GetSkillDescription() + "选择4个，并且以空格隔开\n" +
                      $"Color：史莱姆的颜色，在中" + GetColorsWithDot() + "选择一种\n" +
                      "以你的描述内容，生成一段json代码。\n" +
                      "然后用一段生动的文字，以第二人称视角，描述这只史莱姆与它的训练家('你')见面的场景，注意史莱姆不会说话，字数在200字左右。\n" +
                      "这段文字请以‘Amy：’开头。\n" +
                      "你的能力有:\n" +
                      "- 材料分析:识别并理解用户提供的材料特性。\n" +
                      "- 创造设计:根据材料特性设计史莱姆的外观和属性。\n" +
                      "- 实现构建:将设计转化为具体的史莱姆实体。\n" +
                      "- 生成包含这些信息的json\n\n"
        };

        // 将system prompt作为第一条对话记录加入chatHistory
        chatHistory.Add(systemPrompt);

    }


    private async void SendGLM()
    {
        string slimeName = playerInput.text;
        playerInput.text = "";
        string sendText = "";
        sendText = sendText + "[" + string.Join(",", materials) + "]";
        sendText = sendText + "史莱姆的名字是：" + slimeName;
        //发送
        SendData playerMessage = new SendData()
        {
            role = "user",
            content = sendText
        };
        chatHistory.Add(playerMessage);

        // 使用GLMHandler.GenerateGLMResponse生成GLM回复，设置tmeperature=0.8
        // 注意需要使用await关键词
        SendData respone = await GlmHandler.GenerateGlmResponseV3(chatHistory, 0.8f, null, -1);
        print(respone.content);
        // 将GLM的回复加进chatHistory
        chatHistory.Add(respone);
        responseText = respone.content;

        // 使用response.content获取GLM的回复
        // 使用正则表达式提取 JSON 和描述部分
        string jsonPattern = @"\{[\s\S]*?\}";
        string descriptionPattern = @"Amy：([\s\S]*)";

// 提取 JSON 部分
        Match jsonMatch = Regex.Match(responseText, jsonPattern);
        json = jsonMatch.Success ? jsonMatch.Value : "未找到 JSON 数据";
        print(json);

// 提取描述部分
        Match descriptionMatch = Regex.Match(responseText, descriptionPattern);
        description = descriptionMatch.Success ? descriptionMatch.Groups[1].Value.Trim() : "未找到描述文本";
        print(description);
        UpdateUI();
// 输出提取的结果
        // Console.WriteLine("提取的 JSON 数据：");
        // Console.WriteLine(json);
        // Console.WriteLine("\n提取的描述文本：");
        // Console.WriteLine(description);
        create();


    }

    public void generate()
    {
        SendGLM();
        SoundManager.PlayAudio("spell");
    }

    [Serializable]
    public class SlimeAttributes
    {
        public string Name;
        public int HP;
        public string Type;
        public int Attack;
        public int Defence;
        public int Luck;
        public int Speed;
        public string Personality;
        public int Intimacy;
        public int Grown;
        public string Skill;
        public string Color;
    }

    public static SlimeType getType(string type)
    {
        Dictionary<string, SlimeType> slimeTypeMapping = new Dictionary<string, SlimeType>
        {
            { "Fire", SlimeType.Fire },
            { "Water", SlimeType.Water },
            { "Lightning", SlimeType.Lightning },
            { "Wood", SlimeType.Wood },
            { "None", SlimeType.None }
        };

        static SlimeType MatchSlimeType(string input, Dictionary<string, SlimeType> typeMapping)
        {
            // 精确匹配
            if (typeMapping.ContainsKey(input))
            {
                return typeMapping[input];
            }

            // 模糊匹配
            foreach (var type in typeMapping.Keys)
            {
                if (IsSimilar(input, type))
                {
                    return typeMapping[type];
                }
            }

            // 未找到匹配的类型
            return SlimeType.None;
        }

        static bool IsSimilar(string str1, string str2)
        {
            // 简单的模糊匹配算法：计算编辑距离
            int maxLength = Math.Max(str1.Length, str2.Length);
            int distance = ComputeLevenshteinDistance(str1, str2);

            // 如果编辑距离小于一定比例（如 40%），认为是相似的
            return (double)distance / maxLength < 0.4;
        }

        static int ComputeLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target.Length;
            if (string.IsNullOrEmpty(target)) return source.Length;

            int[,] dp = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
            {
                dp[i, 0] = i;
            }

            for (int j = 0; j <= target.Length; j++)
            {
                dp[0, j] = j;
            }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                }
            }

            return dp[source.Length, target.Length];
        }

        return MatchSlimeType(type, slimeTypeMapping);

    }

    public static Personality getPersonality(string type)
    {
        Dictionary<string, Personality> personalityMapping = new Dictionary<string, Personality>
        {
            { "Brave", Personality.Brave },
            { "Contious", Personality.Contious },
            { "Smart", Personality.Smart },
            { "Calmly", Personality.Calmly },
            { "Friendly", Personality.Friendly }
        };

        static Personality MatchPersonality(string input, Dictionary<string, Personality> typeMapping)
        {
            // 精确匹配
            if (typeMapping.ContainsKey(input))
            {
                return typeMapping[input];
            }

            // 模糊匹配
            foreach (var type in typeMapping.Keys)
            {
                if (IsSimilar(input, type))
                {
                    return typeMapping[type];
                }
            }

            // 未找到匹配的类型
            return Personality.Contious;
        }

        static bool IsSimilar(string str1, string str2)
        {
            // 简单的模糊匹配算法：计算编辑距离
            int maxLength = Math.Max(str1.Length, str2.Length);
            int distance = ComputeLevenshteinDistance(str1, str2);

            // 如果编辑距离小于一定比例（如 40%），认为是相似的
            return (double)distance / maxLength < 0.4;
        }

        static int ComputeLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target.Length;
            if (string.IsNullOrEmpty(target)) return source.Length;

            int[,] dp = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
            {
                dp[i, 0] = i;
            }

            for (int j = 0; j <= target.Length; j++)
            {
                dp[0, j] = j;
            }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                }
            }

            return dp[source.Length, target.Length];
        }

        return MatchPersonality(type, personalityMapping);
    }

    private GameObject go;

    void create()
    {
        SlimeAttributes slimeData = JsonConvert.DeserializeObject<SlimeAttributes>(json);
        if (go != null) Destroy(go);
        go = Instantiate(slimeTemplete, generatePos.position, Quaternion.identity);

        for (var i = 0; i < colors.Length; i++)
        {
            if (slimeData.Color != colors[i]) continue;
            var a = animators[i];
            go.GetComponent<Animator>().runtimeAnimatorController = a;
            break;
        }

        var slime = go.GetComponent<Slime>();
        var b = slime.battleProperties;
        var e = slime.eduProperties;
        b.HP = slimeData.HP;
        b.maxHP = slimeData.HP;
        b.Defence = slimeData.Defence;
        b.Attack = slimeData.Attack;
        b.Speed = slimeData.Speed;
        b.Luck = slimeData.Luck;
        b.type = getType(slimeData.Type);

        e.name = slimeData.Name;
        e.Grown = slimeData.Grown;
        e.Intimacy = slimeData.Intimacy;
        e.Awaken = false;
        e.personality = getPersonality(slimeData.Personality);
        e.isPlayer = true;
        proceedButton.gameObject.SetActive(true);

        var subString = slimeData.Skill.Split(' ');
        foreach (var s in subString)
        {
            var skill = skillList.GetSkillByName(s);
            if (skill != null) slime.LearnSkill(skill);
        }

        SlimeSetSo.CreateSlime(slimeData);

    }

    public void ProceedButton()
    {
        SoundManager.PlayAudio("button");
        StartCoroutine(AsyncLoadScene(1));
    }

    IEnumerator AsyncLoadScene(int sceneName)
    {
        yield return new WaitForSeconds(2f);
        // 异步加载场景
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 确保场景不会自动激活
        if (operation != null)
        {
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                // 当加载进度达到0.9时，认为场景已加载完毕
                if (operation.progress >= 0.9f)
                {
                    // 可以在这里做一些加载完毕前的准备工作

                    // 激活场景
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        //     private void ReciveGLM()
//     {
//         string inputText = "";
//         
// // 使用正则表达式提取 JSON 和描述部分
//         string jsonPattern = @"\{[\s\S]*?\}";
//         string descriptionPattern = @"Amy：([\s\S]*)";
//
// // 提取 JSON 部分
//         Match jsonMatch = Regex.Match(inputText, jsonPattern);
//         string json = jsonMatch.Success ? jsonMatch.Value : "未找到 JSON 数据";
//
// // 提取描述部分
//         Match descriptionMatch = Regex.Match(inputText, descriptionPattern);
//         string description = descriptionMatch.Success ? descriptionMatch.Groups[1].Value.Trim() : "未找到描述文本";
//
// // 输出提取的结果
//         Console.WriteLine("提取的 JSON 数据：");
//         Console.WriteLine(json);
//         Console.WriteLine("\n提取的描述文本：");
//         Console.WriteLine(description);
//
//     }
    }
}




// // 使用GlmFunctionTool类创建一个函数调用工具，并加入函数需要返回的参数
// GlmFunctionTool functionTool = new GlmFunctionTool("forge_weapon", "用户想打造一件武器。根据用户的描述，提取关于这把武器的信息");
// functionTool.AddProperty("weapon_name", "string", "武器名称");
// functionTool.AddProperty("weapon_description", "string", "武器简介");
// // 把武器价格输出的isRequired设置为false。如果玩家输入的文字未提供价格，GLM就不会输出价格
// functionTool.AddProperty("weapon_price", "int", "武器价格", false);
// // 设置属性的enum为"火", "雷", "冰"中的一种
// functionTool.AddProperty("damage_type", "string", "武器造成的伤害的属性", false, new List<string> { "火", "雷", "冰" });
//
// // 发送GLM函数请求，注意需要将functionTool放在一个List<GlmTool>工具列表里
// SendData response = await GlmHandler.GenerateGlmResponse(chat, 0.6f, new List<GlmTool> { functionTool });
//
// // GLM会根据user的输入内容和函数是否相关，决定是使用函数工具还是生成普通对话
// // 如果response.tool_calls != null，则代表GLM使用了函数工具
// if (response.tool_calls != null)
// {
//     // 使用response.tool_calls[0].arguments_dict来获取输出参数
//     Dictionary<string, string> functionOutput = response.tool_calls[0].arguments_dict;
//
//     _weaponNameText.text = functionOutput["weapon_name"];
//     _weaponDescriptionText.text = functionOutput["weapon_description"];
//     // 因为GLM的输出可能不包含武器价格和属性，所以需要额外判断
//     _weaponPriceText.text = functionOutput.ContainsKey("weapon_price") ? functionOutput["weapon_price"] : "";
//     _damageTypeText.text = functionOutput.ContainsKey("damage_type") ? functionOutput["damage_type"] : "";
// }
// // 如果user的输入与函数不相关，GLM会执行普通的对话，不调用函数。
// else
// {
//     _weaponNameText.text = _weaponDescriptionText.text = _weaponPriceText.text = _damageTypeText.text = "";
//     Debug.Log(response.content);
// }
//
