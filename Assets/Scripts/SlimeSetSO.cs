using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "SlimeSet",menuName = "ScriptableObjects/SlimeSet")]
public class SlimeSetSO : ScriptableObject
{
    public List<GameObject> slimes;
    public int maxCount;
    public void AddSlime(GameObject slimeGameObject)
    {
        var slime = slimeGameObject.GetComponent<Slime>();
        if (slimes == null)
        {
            slimes = new List<GameObject>();
            slimes.Add(slimeGameObject);
        }
        else if (slimes.Count == 0)
        {
            slimes.Add(slimeGameObject);
        }
        else
        {
            if (slime.eduProperties.isPlayer)
            {
                if (slimes[0].GetComponent<Slime>().eduProperties.isPlayer)
                {//替换
                    var zero = slimes[0];
                    zero.GetComponent<Slime>().eduProperties.isPlayer = false;
                    slimes[0] = slimeGameObject;
                    slimes.Add(zero);
                }
                else
                {//插入
                    slimes.Insert(0,slimeGameObject);
                }
            }
            else
            {
                slimes.Add(slimeGameObject);
            }
        }
    }
    public GameObject GetPlayerSlime()
    {
        if (slimes == null)
        {
            return null;
        }
        return slimes[0];
    }

    public GameObject GetRandomSlime()
    {
        if(slimes.Count <= 1) return null;
        var s = slimes[Random.Range(1, slimes.Count)];
        return s;
    }

    public void CreateSlime(GameObject go)
    {
        bool dirty=false;
        string path = "Assets/Prefabs/Slimes/" + go.name + slimes.Count + ".prefab";
        if (slimes.Count >= maxCount)
        {
            int i = Random.Range(1, slimes.Count);
            string s = slimes[i].name;
            slimes.RemoveAt(i);
            //替换prefab
            path = "Assets/Prefabs/Slimes/" + s +".prefab";
            dirty = true;
        }
        
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path, out bool success);
        if (!success) return;
        else AddSlime(prefab);
        if (dirty)
        {
            prefab.name = go.name;
        }
    }
}
