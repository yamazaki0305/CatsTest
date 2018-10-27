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

    // ボタンに表示する英語（全て大文字）
    private string EigoText;
    // TransWindowに表示する英単語
    private string TransEigoText;
    // TransWindowに表示する英単語の翻訳
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

    //ゲーム開始前の処理
    private GameObject StartWindow;

    //DBの定義
    SqliteDatabase sqlDB;

    //無視英単語リスト
    string[] ignore_word;

    // リストを作っている
    private List<GameObject> PuzzleDataList = new List<GameObject>();

    // ゲームループフラグを管理
    GameLoopFlg GameFlg = GameLoopFlg.PlayBefore;

    bool isRunning = true;

    void Start()
    {

        //無視英単語リストを設定する
        //　テキストファイルから読み込んだデータ
        TextAsset textasset = new TextAsset(); //テキストファイルのデータを取得するインスタンスを作成
        textasset = Resources.Load("ignore_word", typeof(TextAsset)) as TextAsset; //Resourcesフォルダから対象テキストを取得
        string TextLines = textasset.text; //テキスト全体をstring型で入れる変数を用意して入れる

        //Splitで一行づつを代入した1次配列を作成
        ignore_word = TextLines.Split('\n'); //

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

        // 音声ファイルを設定
        soundTap = Resources.Load("SOUND/SE/cursor1", typeof(AudioClip)) as AudioClip;

    }

    // Update is called once per frame
    void Update()
    {
        // ゲーム開始前処理
        if (GameFlg == GameLoopFlg.PlayBefore)
        {
            // スマホのタッチと、PCのクリック判定
            if (Input.GetMouseButtonDown(0))
            {
                StartWindow.SetActive(false);
                //Vector2 pos = new Vector2(0, -170);
                //TransWindow.transform.position = pos;

                // プレイ中処理に移行
                GameFlg = GameLoopFlg.PlayNow;
            }
            return;
        }
        // 和訳表示中処理
        else if (GameFlg == GameLoopFlg.Translate)
        {

            
            TransWindow.SetActive(true);

            Vector2 pos = new Vector2(0, -580);
            Transform trans = GameObject.Find("UICanvas").GetComponent<Transform>();
            TransWindow.transform.SetParent(trans);
            TransWindow.transform.localPosition = pos;

            Debug.Log("x:" + TransWindow.transform.position.x + "y:" + TransWindow.transform.position.y);


            Text EngText = GameObject.Find("EngWord").GetComponent<Text>();
            EngText.GetComponent<Text>().text = TransEigoText;

            Text JapText = GameObject.Find("JapWord").GetComponent<Text>();

            // スペースを取り除く
            string str = TransText.Replace(" ", "").Replace("　", "");

            //先頭から12行✕3列分の文字列を取得
            if (str.Length > 36)
                str = str.Substring(0, 36);

            JapText.GetComponent<Text>().text = str;

            //スターリワードをチェック
            StarData.StarCheck(EigoText);
            EigoText = "";
            EigoButton.GetComponentInChildren<Text>().text = EigoText;

            var button = EigoButton.GetComponent<Button>();

//            if (Input.GetMouseButtonDown(0))
//            {
            
                StartCoroutine(BreakBlockCoroutine());

                if (isRunning == false)
                {
                    StatusData.Score += EigoText.Length * 10;
                    StatusData.Hand--;
                    StatusUpdate();

                    puzzleObjectGroup.SelectEigoDestroy();

                    btnFlg = ButtonFlg.NORMAL;

                    // ブロック移動中処理に移行
                    GameFlg = GameLoopFlg.BlockMove;
                    isRunning = true;

                    ButtonColorChange(button);

                    //ブロックデータリストをクリア
                    PuzzleDataList.Clear();

                return;
                }
 //           }
        }
        // ブロック移動中処理
        else if (GameFlg == GameLoopFlg.BlockMove)
        {
            // 救出済ねこがいない時、移動中のブロックがない時
            if (puzzleObjectGroup.CheckBlockMove() == false )
            {
                // 和訳の表示をしない
                TransWindow.SetActive(false);

                if (puzzleObjectGroup.DeathCat() == false)
                {
                    GameFlg = GameLoopFlg.PlayNow;
                }
                /*
                audioSource = this.GetComponent<AudioSource>();
                audioSource.clip = soundStar;
                audioSource.Play();
                */
            }
        }
        // ゲーム中処理
        else if (GameFlg == GameLoopFlg.PlayNow)
        {

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
                                PuzzleDataList.Add(collition2d.gameObject);

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
                                int x = PuzzleDataList[PuzzleDataList.Count - 1].GetComponent<BlockData>().X;
                                int y = PuzzleDataList[PuzzleDataList.Count - 1].GetComponent<BlockData>().Y;

                                // 最後にタップしたブロックの選択を解除
                                if (blockData.X == x && blockData.Y == y)
                                {
                                    PuzzleDataList[PuzzleDataList.Count - 1].GetComponent<BlockData>().ChangeBlock(false, false);
                                    PuzzleDataList.RemoveRange(PuzzleDataList.Count - 1, 1);

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

    }

    IEnumerator  BreakBlockCoroutine()
    {
        for (int i = 0; i < PuzzleDataList.Count(); i++)
        {
            PuzzleDataList[i].SetActive(false);
            yield return new WaitForSeconds(0.25f);
        }
        isRunning = false;
    }

bool EigoJudgement()
    {
        //英単語になったかの判定(2文字以上の時)
        bool judge = false;
        //英単語になったときの単語
        string eigoword="temp";

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

                eigoword = word;

                Debug.Log("word:" + word);
                Debug.Log("mean:" + str);

                TransText += str;

            }

            if ( judge == false)
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
                    eigoword = word;

                    TransText += str;

                }
            }
        }
    

        //除外英単語の時はjudge=falseにする
        if (judge)
        {

            for (int i = 0; i < ignore_word.Length; i++)
            {
                //Debug.Log("英単語:" + eigoword + "無視:" + ignore_word[i]);
                if (ignore_word[i].ToString() == eigoword)
                {
                    //Debug.Log("false");
                    judge = false;
                }
            }

        }

        //英単語になった時にボタンの色を変える
        if (judge)
        {
            btnFlg = ButtonFlg.EIGO;
            for (int i = 0; i < PuzzleDataList.Count; i++)
            {
                PuzzleDataList[i].GetComponent<BlockData>().ChangeBlock(true, true);
            }
            puzzleObjectGroup.SelectEigoChange();
            TransEigoText = eigoword;
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
            for (int i = 0; i < PuzzleDataList.Count; i++)
            {
                PuzzleDataList[i].GetComponent<BlockData>().ChangeBlock(true, false);
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

            //ブロックデータリストをクリア
            PuzzleDataList.Clear();
        }
        else if (btnFlg == ButtonFlg.EIGO)
        {


            AudioSource a1;
            AudioClip audio = Resources.Load("SOUND/SE/decision4", typeof(AudioClip)) as AudioClip;
            a1 = gameObject.AddComponent<AudioSource>();
            a1.clip = audio;
            a1.Play();
            /*
            audioSource = this.GetComponent<AudioSource>();
            audioSource.clip = soundOK;
            audioSource.Play();
            */

            // 和訳表示処理に移行
            GameFlg = GameLoopFlg.Translate;

            //ブロックデータリストをクリア
            //blockDataList.Clear();
        }

  

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

            EigoButton.GetComponent<EigoButtonController>().scaling = false;
        }
        else if (btnFlg == ButtonFlg.NORMAL)
        {
            var colors = button.colors;
            colors.normalColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            colors.highlightedColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            colors.pressedColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 255f / 255f);
            colors.disabledColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 255f / 255f);

            EigoButton.GetComponent<Button>().colors = colors;

            EigoButton.GetComponent<EigoButtonController>().scaling = false;
        }
        else if (btnFlg == ButtonFlg.EIGO)
        {
            var colors = button.colors;
            colors.normalColor = new Color(255f / 255f, 255f / 255f, 153f / 255f, 255f / 255f);
            colors.highlightedColor = new Color(255f / 255f, 255f / 255f, 153f / 255f, 255f / 255f);
            colors.pressedColor = new Color(255f / 255f, 255f / 255f, 153f / 255f, 255f / 255f);
            colors.disabledColor = new Color(255f / 255f, 255f / 255f, 153f / 255f, 255f / 255f);

            EigoButton.GetComponent<Button>().colors = colors;

            EigoButton.GetComponent<EigoButtonController>().scaling = true;
            isRunning = true;

        }
    }

    public void StatusUpdate()
    {
        StatusCat.GetComponent<Text>().text = "ねこ:" + StatusData.Cat + "匹";
        StatusHand.GetComponent<Text>().text = "残り:" + StatusData.Hand + "回";
        StatusScore.GetComponent<Text>().text =  StatusData.Score+"点";
    }
}