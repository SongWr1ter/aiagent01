using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MESSAGE_TYPE
{
    GAME_RESTART,
    ADD_SCORE,
    GAME_OVER,
    WIN,
    ENEMY_DEAD
}
public struct CommonMessage
{
    public int Mid;
    public Object content;
    public int intParam;
}
public class MessageCenter
{
    public static bool Enabled = true;
    private static System.Action<CommonMessage> _actions;
    //private static delegate Action XX 是错误的

    public static void AddListener( System.Action<CommonMessage> action)
    {
        _actions += action;
    }

    public static void RemoveListener(System.Action<CommonMessage> action)
    {
        _actions -= action;
    }

    public static void SendMessage(CommonMessage msg)
    {
        if(!Enabled)
            return;
        // if(msg.Mid ==(int) MESSAGE_TYPE.GAME_RESTART)
        // {
        //     double t = Time.realtimeSinceStartupAsDouble;
        //     _actions?.Invoke(msg);
        //     Debug.Log(Time.realtimeSinceStartupAsDouble - t);
        // }
        // else
            _actions?.Invoke(msg);
        
    }

}
