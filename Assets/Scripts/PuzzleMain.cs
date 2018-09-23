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

    private GameObject GameOverObj;
    // Use this for initialization

    //DBの定義
    SqliteDatabase sqlDB;

    // リストを作っている
    private List<BlockData> blockDataList = new List<BlockData>();

    void Start()
    {
        sqlDB = new SqliteDatabase("ejdict.sqlite3");

        GameOverObj = GameObject.Find("GameOverText");
        GameOverObj.GetComponent<Text>().text = "";
        GameOverObj.SetActive(false);

        StatusData = new StageStatus(3, 5);
        StatusUpdate();

        btnFlg = ButtonFlg.NORMAL;

        EigoText = "";
        //EigoText.GetComponent<Text>().text = "";



    }

    // Update is called once per frame
    void Update()
    {

        //ゲームーバー判定
        if (StatusData.Hand == 0)
        {
            GameOverObj.GetComponent<Text>().text = "GameOver!!";
            GameOverObj.SetActive(true);
        }
        //ゲームクリア判定
        else if (StatusData.Cat == 0)
        {
            GameOverObj.GetComponent<Text>().text = "GameClear!!";
            GameOverObj.SetActive(true);
        }

        // スマホのタッチと、PCのクリック判定
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collition2d = Physics2D.OverlapPoint(point);

            BlockData blockData;

            // ここでRayが当たったGameObjectを取得できる
            if (collition2d)
            {
                if (collition2d.tag == "Block")
                {
                    blockData = collition2d.GetComponent<BlockData>();

                    if (blockData.blockType == BlockType.ALPHABET)
                    {
                        if (!blockData.Selected)
                        {
                            blockDataList.Add(blockData);

                            blockData.TapBlock();

                            // ここでRayが当たったGameObjectを取得できる
                            EigoText += blockData.Alphabet;
                            EigoButton.GetComponentInChildren<Text>().text = EigoText;
                            //                            Debug.Log(EigoText);

                            //英単語になったかの判定
                            bool judge = EigoJudgement();

                            //英単語になった時
                            if (judge)
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
                        else
                        {
                            int x = blockDataList[blockDataList.Count - 1].X;
                            int y = blockDataList[blockDataList.Count - 1].Y;

                            // 最後にタップしたブロックの選択を解除
                            if (blockData.X == x && blockData.Y == y)
                            {
                                blockDataList[blockDataList.Count - 1].ChangeBlock(false,false);
                                blockDataList.RemoveRange(blockDataList.Count-1, 1);

                                //末尾から1文字削除する
                                EigoText = EigoText.Remove(EigoText.Length - 1, 1);
                                EigoButton.GetComponentInChildren<Text>().text = EigoText;
                                EigoJudgement();



                            }
                        }
                    }
                }
            }
        }

    }

    bool EigoJudgement()
    {
        //英単語になったかの判定は2文字以上から
        bool judge = false;
        if (EigoText.Length >= 2)
        {
            string eigo = EigoText.ToLowerInvariant();
            string query = "select word,mean from items where word ='" + eigo + "'";
            DataTable dataTable = sqlDB.ExecuteQuery(query);
            foreach (DataRow dr in dataTable.Rows)
            {
                judge = true;
                string word = (string)dr["word"];
                string str = (string)dr["mean"];
                // attack = (int)dr["attack"];
                Debug.Log("word:" + word);
                Debug.Log("mean:" + str);

            }
        }
        //英単語になった時=現在は４文字以上で英単語と判定する
        if (judge)
        {
            btnFlg = ButtonFlg.EIGO;
            for (int i = 0; i < blockDataList.Count; i++)
            {
                blockDataList[i].ChangeBlock(true, true);
            }
            puzzleObjectGroup.SelectEigoChange();


        }
        //英単語ではない
        else
        {
            btnFlg = ButtonFlg.PRESSED;
            for (int i = 0; i < blockDataList.Count; i++)
            {
                blockDataList[i].ChangeBlock(true, false);
            }
        }
        var button = EigoButton.GetComponent<Button>();
        ButtonColorChange(button);

        return judge;
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

        //ブロックデータリストをクリア
        blockDataList.Clear();

    }

    void ButtonColorChange(Button button)
    {
        if (btnFlg == ButtonFlg.PRESSED)
        {
            var colors = button.colors;
            colors.normalColor = new Color(255f / 255f, 51f / 255f, 153f / 255f, 255f / 255f);
            colors.highlightedColor = new Color(255f / 255f, 51f / 255f, 153f / 255f, 255f / 255f);
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
        StatusCat.GetComponent<Text>().text = "ねこ:" + StatusData.Cat + "匹";
        StatusHand.GetComponent<Text>().text = "残り:" + StatusData.Hand + "回";
        //StatusScore.GetComponent<Text>().text =  StatusData.Score+"点";
    }
}