using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class bubbleInstant : MonoBehaviour
{
    SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.DOFade(0f, 1.0f).SetAutoKill().OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

   
}
