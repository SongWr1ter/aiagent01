using UnityEngine;

[System.Serializable]
public enum SkillType
{
    ATTACK,
    BUFF
}
public class Skill
{
    public string name;
    public SkillType skillType; 
    public SlimeType type;//ˮ��ľ��
    public float number;//�����ͼ������˺��������ͼ����ǰٷֱ�
    public int count;//ʹ�ô���
    /// <summary>
    /// �ͷż���
    /// </summary>
    /// <param name="user"></param>
    /// <param name="obj"></param>
    /// <returns>obj�Ƿ�����</returns>
    public virtual bool Discharge(Slime user, Slime obj)
    {
        if (--number < 0)
        {
            Debug.Log($"{user.eduProperties.name}�ͷ���{name},��û���ͷųɹ�");
        }
        else
        {
            Debug.Log($"{user.eduProperties.name}��{obj.eduProperties.name}�ͷ���{name}");
        }
        return false;
        
    }
}