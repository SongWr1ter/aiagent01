using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nDebuger : MonoBehaviour
{

    public Slime A;
    public Slime B;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            //GameManager.Instance.SlimeAction(A, B, "skill499");
        }
    }
}
