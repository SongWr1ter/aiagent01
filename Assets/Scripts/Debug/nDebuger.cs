using System.IO;
using UnityEditor;
using UnityEngine;

public class nDebuger : MonoBehaviour
{
    public StatePanelItem PlayerPanelItem;
    public StatePanelItem AIPanelItem;
    public Slime A;
    public Slime B;
    public TextPanel TextPanel;
    [TextArea]
    public string TextArea;

    public SlimeSetSO so;
    public GameObject selectedGameObject;
    bool flag = false;
    int counter2 = 0;
    private void Start()
    {
        // PlayerPanelItem.Init(A);
        // AIPanelItem.Init(B);
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space) && flag == false)
        // {
        //     GameManager.Instance.Setup();
        //     flag = !flag;
        //     //TextPanel.StartTyping(TextArea);
        //     //GameManager.Instance.Setup();
        //     //GameManager.Instance.Action(A, B);
        // }
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            counter2++;
            if (counter2 == 3)
            {
                Application.Quit();
            }
        }

        // if (Input.GetKeyDown(KeyCode.RightAlt))
        // {
        //     counter2++;
        //     if (counter2 >= 5)
        //     {
        //         counter2 = 0;
        //         so.slimes.Clear();
        //         string folderPath = "Assets/Prefabs/Slimes/";
        //         // 检查文件夹是否存在
        //         if (Directory.Exists(folderPath))
        //         {
        //             // 获取文件夹内所有文件
        //             string[] files = System.IO.Directory.GetFiles(folderPath, "*.prefab", SearchOption.AllDirectories);
        //
        //             foreach (string file in files)
        //             {
        //                 // 删除文件
        //                 File.Delete(file);
        //                 Debug.Log("已删除: " + file);
        //             }
        //
        //             Debug.Log("所有.prefab文件已删除");
        //             
        //             // files = System.IO.Directory.GetFiles(folderPath, "*.meta", SearchOption.AllDirectories);
        //             //
        //             // foreach (string file in files)
        //             // {
        //             //     // 删除文件
        //             //     File.Delete(file);
        //             //     Debug.Log("已删除: " + file);
        //             // }
        //             //
        //             // Debug.Log("所有.meta文件已删除");
        //         }
        //         else
        //         {
        //             Debug.LogError("文件夹不存在: " + folderPath);
        //         }
        //         
        //     }
        //     
        // }
    }
}
