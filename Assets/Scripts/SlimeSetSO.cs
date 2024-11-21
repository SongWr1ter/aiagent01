using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "SlimeSet",menuName = "ScriptableObjects/SlimeSet")]
public class SlimeSetSO : ScriptableObject
{
    public List<CrucibleManager.SlimeAttributes> slimes;
    public int maxCount;
    public GameObject prefab;
    public SkillList skillList;
    public List<RuntimeAnimatorController> animators = new List<RuntimeAnimatorController>();

    public void AddSlime(CrucibleManager.SlimeAttributes s)
    {
        if (slimes == null)
        {
            slimes = new List<CrucibleManager.SlimeAttributes>();
            slimes.Add(s);
        }
        else if (slimes.Count == 0)
        {
            slimes.Add(s);
        }
        else
        {
            slimes.Insert(0,s);
            // if (isPlayer)
            // {
            //     if (slimes[0].GetComponent<Slime>().eduProperties.isPlayer)
            //     {//替换
            //         var zero = slimes[0];
            //         zero.GetComponent<Slime>().eduProperties.isPlayer = false;
            //         slimes[0] = s;
            //         slimes.Add(zero);
            //     }
            //     else
            //     {//插入
            //         
            //     }
            // }
            // else
            // {
            //     
            // }
        }
    }
    public GameObject GetPlayerSlime()
    {
        if (slimes == null)
        {
            return null;
        }
        return GenerateSlimeGO(slimes[0],true);
    }

    public GameObject GetRandomSlime()
    {
        if(slimes.Count <= 1) return null;
        var sf = slimes[Random.Range(1, slimes.Count)];
        return GenerateSlimeGO(sf,false);
    }

    public void CreateSlime(CrucibleManager.SlimeAttributes go)
    { 
        //bool dirty=false;
        // string path = "Assets/Prefabs/Slimes/" + go.GetComponent<Slime>().eduProperties.name  + ".prefab";
        // if (slimes.Count >= maxCount)
        // {
        //     int i = Random.Range(1, slimes.Count);
        //     string s = slimes[i].name;
        //     slimes.RemoveAt(i);
        //     //替换prefab
        //     path = "Assets/Prefabs/Slimes/" + s +".prefab";
        //     dirty = true;
        // }
        
        //var prefab = PrefabUtility.SaveAsPrefabAsset(go, path, out bool success);
        // if (!success) return;
        // else AddSlime(prefab);
        // if (dirty)
        // {
        //     prefab.name = go.name;
        // }
        if (slimes.Count >= maxCount)
        {
            int i = Random.Range(1, slimes.Count);
            slimes.RemoveAt(i);
            //dirty = true;
        }
        
        AddSlime(go);
    }

    public GameObject GenerateSlimeGO(CrucibleManager.SlimeAttributes slimeData,bool isPlayer)
    {
        GameObject go = Instantiate(prefab);

        for (var i = 0; i < CrucibleManager.colors.Length; i++)
        {
            if (slimeData.Color != CrucibleManager.colors[i]) continue;
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
        b.type = CrucibleManager.getType(slimeData.Type);

        e.name = slimeData.Name;
        e.Grown = slimeData.Grown;
        e.Intimacy = slimeData.Intimacy;
        e.Awaken = false;
        e.personality = CrucibleManager.getPersonality(slimeData.Personality);
        e.isPlayer = isPlayer;

        var subString = slimeData.Skill.Split(' ');
        foreach (var s in subString)
        {
            var skill = skillList.GetSkillByName(s);
            if(skill != null) slime.LearnSkill(skill);
        }

        return go;
    }
}
