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
    private float magnification = 0.025f;

    //拡大縮小
    public bool scaling;

    int fontsize;
    float alpha;

    // Use this for initialization
    void Start()
    {
        alpha = 1.0f;
        fontsize = this.GetComponent<TextMesh>().fontSize;
        scaling = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (scaling)
        {
            fontsize++;
            alpha += -0.015f;
            this.GetComponent<TextMesh>().fontSize = fontsize;
            this.GetComponent<TextMesh>().color = new Color(255f / 255f, 255f / 255f, 0f / 255f, alpha);
        }

    }
}
