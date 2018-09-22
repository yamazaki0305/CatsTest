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

public class StageStatus
{
    public int Cat;
    public int Hand;
    public int Score;

    //ヘッダーに表示するステータスのclass
    // コンストラクタでインスタンスを生成した時に情報を渡す
    public StageStatus(int cat, int hand)
    {
        this.Cat = cat;
        this.Hand = hand;
        this.Score = 0;
    }
}

public class PuzzleMain : MonoBehaviour
{

    public GameObject EigoButton;

    //ヘッダーStatusのアタッチ
    public GameObject StatusCat;
    public GameObject StatusHand;
    public GameObject StatusScore;

    // パズルオブジェクトグループコンポーネント
    [SerializeField]
    PuzzleObjectGroup puzzleObjectGroup = null;

    private string EigoText;
    // EigoTextの状態を保持(NORMAL/PRESSED/EIGO)
    public ButtonFlg btnFlg;

    public StageStatus StatusData;

    // Use this for initialization
    void Start()
    {

        StatusData = new StageStatus(3, 10);
        StatusUpdate();

        btnFlg = ButtonFlg.NORMAL;

        EigoText = "";
        //EigoText.GetComponent<Text>().text = "";



    }

    // Update is called once per frame
    void Update()
    {

        // スマホのタッチと、PCのクリック判定
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collition2d = Physics2D.OverlapPoint(point);

            // ここでRayが当たったGameObjectを取得できる
            if (collition2d)
            {
                if (collition2d.tag == "Block")
                {

                    if (collition2d.GetComponent<BlockData>().blockType == BlockType.ALPHABET)
                    {
                        if (!collition2d.GetComponent<BlockData>().Selected)
                        {
                            collition2d.GetComponent<BlockData>().TapBlock();

                            // ここでRayが当たったGameObjectを取得できる
                            EigoText += collition2d.gameObject.GetComponent<BlockData>().Alphabet;
                            EigoButton.GetComponentInChildren<Text>().text = EigoText;
                            Debug.Log(EigoText);

                            //英単語になったかの判定分岐
                            //英単語になった時=現在は４文字以上で英単語と判定する
                            if(EigoText.Length >= 4)
                            {
                                btnFlg = ButtonFlg.EIGO;
                                puzzleObjectGroup.SelectEigoChange();
                                

                            }
                            //英単語ではない
                            else
                            {
                                btnFlg = ButtonFlg.PRESSED;
                            }
                            var button = EigoButton.GetComponent<Button>();
                            ButtonColorChange(button);

                        }

                    }
                }
            }
        }

    }

    public void ReloadButton()
    {
        SceneManager.LoadScene("PuzzleGame");
    }
    public void PressEigoButton()
    {
        var button = EigoButton.GetComponent<Button>();

        if (btnFlg == ButtonFlg.PRESSED)
        {
            EigoText = "";
            EigoButton.GetComponentInChildren<Text>().text = EigoText;
            puzzleObjectGroup.SelectAllCanceled();
            btnFlg = ButtonFlg.NORMAL;

            ButtonColorChange(button);
        }
        else if (btnFlg == ButtonFlg.EIGO)
        {
            StatusData.Score += EigoText.Length * 10;
            StatusData.Hand--;
            StatusUpdate();
            EigoText = "";
            EigoButton.GetComponentInChildren<Text>().text = EigoText;
            puzzleObjectGroup.SelectEigoDestroy();
            btnFlg = ButtonFlg.NORMAL;

            ButtonColorChange(button);
        }
        /*
        Debug.Log("ログ"+ puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().blockData[0,0].GetComponent<BlockData>().Alphabet);
        puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().blockData[0, 0].GetComponent<BlockData>().Alphabet = "B";
        puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().blockData[0, 0].GetComponentInChildren<TextMesh>().text = puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().blockData[0, 0].GetComponent<BlockData>().Alphabet;
        //Debug.Log("aaa" + obj.blockData[0][0].GetComponent<BlockData>.X);

        Vector2 pos = new Vector2(0 * 90 - 320 + 45, 0 * 90 - 270);
        puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().blockData[0,0].transform.SetParent(puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().puzzleTransform);
        puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().blockData[0,0].transform.position = pos;
        puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().blockData[0,0].transform.localScale = puzzleObjectGroup.GetComponent<PuzzleObjectGroup>().puzzlePrefab.transform.localScale;
        */

    }

    void ButtonColorChange(Button button)
    {
        if (btnFlg == ButtonFlg.PRESSED)
        {
            var colors = button.colors;
            colors.normalColor = new Color(204f / 255f, 0f / 255f, 0f / 255f, 255f / 255f);
            colors.highlightedColor = new Color(204f / 255f, 0f / 255f, 0f / 255f, 255f / 255f);
            colors.pressedColor = new Color(204f / 255f, 0f / 255f, 0f / 255f, 255f / 255f);
            colors.disabledColor = new Color(204f / 255f, 0f / 255f, 0f / 255f, 255f / 255f);

            EigoButton.GetComponent<Button>().colors = colors;
        }
        else if (btnFlg == ButtonFlg.NORMAL)
        {
            var colors = button.colors;
            colors.normalColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            colors.highlightedColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            colors.pressedColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 255f / 255f);
            colors.disabledColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 255f / 255f);

            EigoButton.GetComponent<Button>().colors = colors;
        }
        else if (btnFlg == ButtonFlg.EIGO)
        {
            var colors = button.colors;
            colors.normalColor = new Color(255f / 255f, 255f / 255f, 153f / 255f, 255f / 255f);
            colors.highlightedColor = new Color(255f / 255f, 255f / 255f, 153f / 255f, 255f / 255f);
            colors.pressedColor = new Color(255f / 255f, 255f / 255f, 153f / 255f, 255f / 255f);
            colors.disabledColor = new Color(255f / 255f, 255f / 255f, 153f / 255f, 255f / 255f);

            EigoButton.GetComponent<Button>().colors = colors;



        }
    }

    public void StatusUpdate()
    {
        StatusCat.GetComponent<Text>().text = "ねこ:" + StatusData.Cat+"匹";
        StatusHand.GetComponent<Text>().text = "残り:" + StatusData.Hand+"回";
        StatusScore.GetComponent<Text>().text =  StatusData.Score+"点";
    }
}
