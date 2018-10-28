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

// ボタンのタイプを定義
public enum ButtonFlg
{
    NORMAL = 1,
    PRESSED = 2,
    EIGO = 3,
}

// ゲームループを定義
public enum GameLoopFlg
{
    PlayBefore, //ステージ紹介中
    PlayNow,    //プレイ中
    Translate, //和訳表示中（ブロックを消す）
    BlockMove,  //ブロック移動中
    PlayEnd,    //プレイ終了（クリア、ゲームオーバー）
    Pause,      //一時停止中
}

public class StageStatus
{
    public int Cat;
    public int Hand;
    public int Score;
    public int star;

    //ヘッダーに表示するステータスのclass
    // コンストラクタでインスタンスを生成した時に情報を渡す
    public StageStatus(int cat, int hand)
    {
        this.Cat = cat;
        this.Hand = hand;
        this.Score = 0;
        this.star = 0;
        StatusUpdate();
    }

    public void CatUpdate()
    {
        this.Cat--;
        StatusUpdate();
    }
    public void HandScoreUpdate(string eigotext)
    {
        this.Hand--;

        int s = 10;

        for (int i = 0; i < eigotext.Length; i++)
        {
            s = s + 10 * i;
        }
        this.Score += s;
        StatusUpdate();
    }
    public void StatusUpdate()
    {

        GameObject StatusCat = GameObject.Find("CatText");
        StatusCat.GetComponent<Text>().text = "ねこ:" + this.Cat + "匹";
        GameObject StatusHand = GameObject.Find("HandText");
        StatusHand.GetComponent<Text>().text = "残り:" + this.Hand + "回";
        GameObject StatusScore = GameObject.Find("ScoreText");
        StatusScore.GetComponent<Text>().text = this.Score + "点";
    }
}

public class StarReword
{

    public string[,] starWord;
    public int star_count;
    public bool[] bstar;
    public string starJaptext;
    public GameObject[] StarObjStar;

    public StarReword(string estr0, string jstr0, string estr1, string jstr1, string estr2, string jstr2)
    {
        starWord = new string[3, 2];
        bstar = new bool[3];
        StarObjStar = new GameObject[3];

        star_count = 0;

        bstar[0] = false;
        starWord[0, 0] = estr0;
        starWord[0, 1] = jstr0;
        bstar[1] = false;
        starWord[1, 0] = estr1;
        starWord[1, 1] = jstr1;
        bstar[2] = false;
        starWord[2, 0] = estr2;
        starWord[2, 1] = jstr2;

        starJaptext = "";

        GameObject StarText = GameObject.Find("StarText");

        for (int i = 0; i < 3; i++)
        {
            if (!bstar[i])
            {
                starJaptext += starWord[i, 1];
                starJaptext += "\n";
            }
        }

        StarText.GetComponent<Text>().text = starJaptext;
        Debug.Log(starJaptext);

        StarObjStar[0] = GameObject.Find("StarObj0/Star");
        StarObjStar[0].SetActive(false);
        StarObjStar[1] = GameObject.Find("StarObj1/Star");
        StarObjStar[1].SetActive(false);
        StarObjStar[2] = GameObject.Find("StarObj2/Star");
        StarObjStar[2].SetActive(false);
        //StarObj[0].Get("Star");


    }
    public bool StarCheck(string eigo)
    {
        string EigoText = eigo;
        bool b = false;
        bool bAdd = false;

        for (int i = 0; i < 3; i++)
        {
            if (eigo == starWord[i, 0])
            {
                if (!bstar[i])
                {
                    bstar[i] = true;
                    string str = starWord[i, 1];
                    starWord[i, 1] = "<color='#FF9900'><b>" + str + "</b></color>";
                    star_count++;
                    bAdd = true;

                }
                b = true;
            }
        }

        if (bAdd)
        {
            for (int i = 0; i < star_count; i++)
            {
                StarObjStar[i].SetActive(true);
            }

            starJaptext = "";
            for (int i = 0; i < 3; i++)
            {
                starJaptext += starWord[i, 1];
                starJaptext += "\n";
            }
            GameObject StarText = GameObject.Find("StarText");
            StarText.GetComponent<Text>().text = starJaptext;
        }

        return b;

    }

}
