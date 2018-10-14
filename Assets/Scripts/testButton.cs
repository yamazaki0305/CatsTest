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

public class testButton : MonoBehaviour {


    //最小サイズ
    private float minimum = 1.0f;
    //拡大縮小スピード
    private float magSpeed = 10.0f;
    //拡大率
    private float magnification = 0.025f;

    //拡大縮小
    public bool scaling;

    // Use this for initialization
    void Start()
    {
        scaling = false;
    }

    public void OnClick()
    {
        Debug.Log("Click");

    }

    // Update is called once per frame
    void Update()
    {

        if (scaling)
        {
            //ボタンを拡大縮小
            this.transform.localScale = new Vector2(this.minimum + Mathf.Sin(Time.time * this.magSpeed) * this.magnification, this.transform.localScale.y);
        }
        else
        {
            this.transform.localScale = new Vector2(1, 1);
        }

    }
}