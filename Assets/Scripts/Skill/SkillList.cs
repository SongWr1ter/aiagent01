using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillList", menuName = "ScriptableObjects/SkillList", order = 1)]
public class SkillList : ScriptableObject
{
    public List<Skill> skillTable = new List<Skill>();

    public Skill GetSkillByName(string name)
    {
        foreach (var skill in skillTable)
        {
            if (skill.name == name)
            {
                return skill;
            }
        }
        return null;
    }

    public void AddSkill(Skill skill)
    {
        skillTable.Add(skill);
    }
}
