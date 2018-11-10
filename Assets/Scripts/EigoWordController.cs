using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EigoWordController : MonoBehaviour {


    //最小サイズ
    private float minimum = 0.1f;
    //拡大縮小スピード
    private float magSpeed = 5.0f;
    //拡大率
    private float magnification = 0.005f;

    //拡大縮小
    public bool scalingFlg;

    //消える時
    public bool breakFlg;

    int fontsize;
    float alpha;

    // Use this for initialization
    void Start()
    {
        alpha = 1.0f;
        fontsize = this.GetComponent<TextMesh>().fontSize;
        scalingFlg = false;
        breakFlg = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (breakFlg)
        {
            fontsize++;
            alpha -= Time.deltaTime; // Time.deltaTime=フレームレード
            this.GetComponent<TextMesh>().fontSize = fontsize;
            this.GetComponent<TextMesh>().color = new Color(255f / 255f, 255f / 255f, 0f / 255f, alpha);
        }

        if (scalingFlg)
        {

            float d = Mathf.Sin(Time.time * this.magSpeed);
            if (d < 0) d = -d;
            //文字を拡大縮小
            this.transform.localScale = new Vector2(this.minimum + d * this.magnification, this.minimum + d * this.magnification);
        }
        else
        {
            this.transform.localScale = new Vector2(0.1f, 0.1f);
        }

    }
}
