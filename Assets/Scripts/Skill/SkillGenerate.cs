using System;
using System.Collections.Generic;
using UnityEngine;
using GlmUnity;
using TMPro;

public class SkillGenerate : MonoBehaviour
{
    public SkillList  skillList;
    public TMP_InputField inputField;
    public List<SendData>  chatList = new List<SendData>();
    private void Start()
    {
        SendData sendData = new SendData
        {
            role = "system",
            content = "请你根据我的输入，将其创意性地转换为一个游戏中的技能，你的回复要求必须含有以下六个部分:" +
                      "技能名字、技能描述、技能类型(有攻击型、提升攻击型、提升速度型、提升生命型、提升幸运型、提升防御型)" +
                      "、技能属性(有水火木雷四种)、技能数值(当技能类型为攻击型时代表伤害倍率(它应该大于1),否则代表属性提升倍率(它应该小于1))、技能使用次数(取值在1到5之间，越强大的技能使用次数越少)\n"
        };
        chatList.Add(sendData);
    }
    
    public static List<string> SplitStringIntoRandomSubstrings(string input, int numberOfSubstrings)
    {
        System.Random random = new System. Random();
        int stringLength = input.Length;
        List<int> splitPoints = new List<int>();
 
        while (splitPoints.Count < numberOfSubstrings - 1)
        {
            int point = random.Next(1, stringLength); // 生成一个介于1和stringLength - 1之间的随机点
            if (!splitPoints.Contains(point))
            {
                splitPoints.Add(point);
            }
        }
 
        splitPoints.Sort(); // 将切分点排序以确保它们按升序排列
 
        List<string> substrings = new List<string>();
        int previousPoint = 0;
 
        for (int i = 0; i < splitPoints.Count; i++)
        {
            substrings.Add(input.Substring(previousPoint, splitPoints[i] - previousPoint));
            previousPoint = splitPoints[i];
        }
 
        // 添加最后一个子串
        substrings.Add(input.Substring(previousPoint, stringLength - previousPoint));
 
        return substrings;
    }

    public async void CreateAvatar()
    {
        // 读取玩家输入并创建用户信息
        string playerInput = inputField.text;
        inputField.text = "";
        SendData chat = new SendData()
            {
                role = "user", content = playerInput
            };
            chatList.Add(chat);
            // 使用GlmFunctionTool类创建一个函数调用工具，并加入函数需要返回的参数
            GlmFunctionTool functionTool = new GlmFunctionTool("generate_skill", "用户想生成一个技能。根据用户的描述，提取关于这个武器的信息");
            functionTool.AddProperty("name", "string", "技能名称");
            functionTool.AddProperty("description", "string", "技能简介");
            functionTool.AddProperty("skill_type", "string", "技能类型",true,new List<string>{"ATTACK","BUFF_LUCK","BUFF_ATTACK","BUFF_DEFEND","BUFF_SPEED","BUFF_HP"});
            functionTool.AddProperty("slime_type", "string", "技能造成的伤害的属性", true, new List<string> { "Water", "Fire", "Wood","Lightning" });
            functionTool.AddProperty("number","float","当技能类型为ATTACK时代表伤害倍率(它应该大于1),否则代表属性提升倍率(它应该小于1)");
            functionTool.AddProperty("count","int","技能的使用次数");

            // 发送GLM函数请求，注意需要将functionTool放在一个List<GlmTool>工具列表里
            SendData response = await GlmHandler.GenerateGlmResponse(chatList, 0.6f, new List<GlmTool> { functionTool });

            // GLM会根据user的输入内容和函数是否相关，决定是使用函数工具还是生成普通对话
            // 如果response.tool_calls != null，则代表GLM使用了函数工具
            if (response.tool_calls != null)
            {
                // 使用response.tool_calls[0].arguments_dict来获取输出参数
                Dictionary<string, string> functionOutput = response.tool_calls[0].arguments_dict;
                chatList.Add(response);
                string name = functionOutput["name"];
                string description = functionOutput["description"];
                string slime_type = functionOutput["slime_type"];
                string skill_type = functionOutput["skill_type"];
                string number = functionOutput["number"];
                string count = functionOutput["count"];
                Enum.TryParse<SkillType>(skill_type, out SkillType skillType);
                Enum.TryParse<SlimeType>(slime_type,out SlimeType type);
                if (skillType != SkillType.ATTACK)
                {
                    type = SlimeType.None;
                }
                Skill skill = new Skill(name,description, skillType,type,float.Parse(number),int.Parse(count));
                skillList.AddSkill(skill);
            }
            else
            {
                print(response.content);
            }
        
    }
}
