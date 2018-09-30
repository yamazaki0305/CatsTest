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
using System.Globalization;

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
    public int star;

    //ヘッダーに表示するステータスのclass
    // コンストラクタでインスタンスを生成した時に情報を渡す
    public StageStatus(int cat, int hand)
    {
        this.Cat = cat;
        this.Hand = hand;
        this.Score = 0;
        this.star = 0;
    }
}

public class StarReword
{

    public string[,] starWord;
    public int star_count;
    public bool[] bstar;
    public string starJaptext;
    public GameObject[] StarObjStar;

    public StarReword(string estr0,string jstr0, string estr1, string jstr1, string estr2, string jstr2)
    {
        starWord = new string[3,2];
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

        for(int i=0;i<3;i++)
        {
            if(!bstar[i])
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

        for(int i=0;i<3; i++)
        {
            if( eigo == starWord[i, 0])
            {
                if(!bstar[i])
                {
                    bstar[i] = true;
                    string str = starWord[i, 1];
                    starWord[i, 1] = "<color='#FF9900'><b>" + str+"</b></color>";
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

public class PuzzleMain : MonoBehaviour
{
    // SEを所得
    public AudioClip soundBlockBreak;
    public AudioClip soundClear;
    public AudioClip soundStar;
    public AudioClip soundTap;
    public AudioClip soundOK;
    public AudioClip soundCancel;
    private AudioSource audioSource;

    public GameObject EigoButton;

    //ヘッダーStatusのアタッチ
    public GameObject StatusCat;
    public GameObject StatusHand;
    public GameObject StatusScore;

    // パズルオブジェクトグループコンポーネント
    [SerializeField]
    PuzzleObjectGroup puzzleObjectGroup = null;

    private string EigoText;
    private string TransText;
    // EigoTextの状態を保持(NORMAL/PRESSED/EIGO)
    public ButtonFlg btnFlg;

    public StageStatus StatusData;
    public StarReword StarData;

    //ゲームオーバークリアの表示
    private GameObject GameOverObj;
    // Use this for initialization

    //英訳のWindow
    private GameObject TransWindow;
    bool TransWindowflg = false;

    //ゲーム開始前の処理
    private GameObject StartWindow;
    bool bStartFlg = true;

    //DBの定義
    SqliteDatabase sqlDB;

    // リストを作っている
    private List<BlockData> blockDataList = new List<BlockData>();

    void Start()
    {

        //★ミッションの設定
        StarData = new  StarReword("ANIMAL", "[名]動物", "PEOPLE", "[名]人々", "CHECK", "[動]確認する");

       //DBの設定
       sqlDB = new SqliteDatabase("ejdict.sqlite3");

        //GameObjectを探して格納
        TransWindow = GameObject.Find("TransWindow");
        TransWindow.SetActive(false);
        StartWindow = GameObject.Find("StartWindow");
        StartWindow.SetActive(true);

        GameOverObj = GameObject.Find("GameOverText");
        GameOverObj.GetComponent<Text>().text = "";
        GameOverObj.SetActive(false);

        StatusData = new StageStatus(3, 10);
        StatusUpdate();

        btnFlg = ButtonFlg.NORMAL;

        EigoText = "";
        TransText = "";
        //EigoText.GetComponent<Text>().text = "";



    }

    // Update is called once per frame
    void Update()
    {
        if (bStartFlg)
        {
            // スマホのタッチと、PCのクリック判定
            if (Input.GetMouseButtonDown(0))
            {
                StartWindow.SetActive(false);
                //Vector2 pos = new Vector2(0, -170);
                //TransWindow.transform.position = pos;

                bStartFlg = false;
            }
            return;
        }
        else if (TransWindowflg)
        {

            TransWindow.SetActive(true);
            Vector2 pos = new Vector2(0,-70);
            TransWindow.transform.position = pos;
            //Transform trans = GameObject.Find("UICanvas").GetComponent<Transform>();
            //TransWindow.transform.SetParent(trans);

            Debug.Log("z:" + TransWindow.transform.position.z);

            Text EngText = GameObject.Find("EngWord").GetComponent<Text>();
            EngText.GetComponent<Text>().text = EigoText;
            Text JapText = GameObject.Find("JapWord").GetComponent<Text>();
            JapText.GetComponent<Text>().text = TransText;

            //スターリワードをチェック
            StarData.StarCheck(EigoText);


            // スマホのタッチと、PCのクリック判定
            if (Input.GetMouseButtonDown(0))
            {

                audioSource = this.GetComponent<AudioSource>();
                audioSource.clip = soundBlockBreak;
                audioSource.Play();

                TransWindowflg = false;
                TransWindow.SetActive(false);


                var button = EigoButton.GetComponent<Button>();

                StatusData.Score += EigoText.Length * 10;
                StatusData.Hand--;
                StatusUpdate();
                EigoText = "";
                EigoButton.GetComponentInChildren<Text>().text = EigoText;
                puzzleObjectGroup.SelectEigoDestroy();
                btnFlg = ButtonFlg.NORMAL;

                ButtonColorChange(button);
            }

            return;
        }

        // 救出済ねこがいないか判定
        if( puzzleObjectGroup.DeathCat() )
        {
            audioSource = this.GetComponent<AudioSource>();
            audioSource.clip = soundStar;
            audioSource.Play();
        }

        //ゲームクリア判定
        if (StatusData.Cat == 0)
        {
            GameOverObj.GetComponent<Text>().text = "GameClear!!";
            GameOverObj.SetActive(true);

        }
        //ゲームーバー判定
        else if (StatusData.Hand == 0)
        {
            GameOverObj.GetComponent<Text>().text = "GameOver!!";
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
                    audioSource = this.GetComponent<AudioSource>();
                    audioSource.clip = soundTap;
                    audioSource.Play();

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

            TransText = "";
            foreach (DataRow dr in dataTable.Rows)
            {
                judge = true;
                string word = (string)dr["word"];
                string str = (string)dr["mean"];
                // attack = (int)dr["attack"];
                Debug.Log("word:" + word);
                Debug.Log("mean:" + str);

                TransText += str;

            }
            if( judge == false)
            {
                //全て小文字にする
                string inStr = EigoText.ToLowerInvariant();

                //1文字目を大文字で検索
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                string outStr = ti.ToTitleCase(inStr);

                query = "select word,mean from items where word ='" + outStr + "'";
                dataTable = sqlDB.ExecuteQuery(query);
                TransText = "";
                foreach (DataRow dr in dataTable.Rows)
                {
                    judge = true;
                    string word = (string)dr["word"];
                    string str = (string)dr["mean"];
                    // attack = (int)dr["attack"];
                    Debug.Log("word:" + word);
                    Debug.Log("mean:" + str);

                    TransText += str;

                }
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
        else if (EigoText.Length == 0)
        {

            btnFlg = ButtonFlg.NORMAL;

        }
        //英単語ではない
        else
        {
            TransText = "";
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
            audioSource = this.GetComponent<AudioSource>();
            audioSource.clip = soundCancel;
            audioSource.Play();

            EigoText = "";
            EigoButton.GetComponentInChildren<Text>().text = EigoText;
            puzzleObjectGroup.SelectAllCanceled();
            btnFlg = ButtonFlg.NORMAL;

            ButtonColorChange(button);
        }
        else if (btnFlg == ButtonFlg.EIGO)
        {
            audioSource = this.GetComponent<AudioSource>();
            audioSource.clip = soundOK;
            audioSource.Play();

            TransWindowflg = true;
            /*
            StatusData.Score += EigoText.Length * 10;
            StatusData.Hand--;
            StatusUpdate();
            EigoText = "";
            EigoButton.GetComponentInChildren<Text>().text = EigoText;
            puzzleObjectGroup.SelectEigoDestroy();
            btnFlg = ButtonFlg.NORMAL;

            ButtonColorChange(button);
            */
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
        StatusScore.GetComponent<Text>().text =  StatusData.Score+"点";
    }
}