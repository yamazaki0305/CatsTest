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


public class PuzzleMain : MonoBehaviour
{
    // SEを所得
    private AudioClip soundTap;
    private AudioClip soundCancel;
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

    // 画面に表示されないブロックの縦数を表示するGameObject
    private GameObject UnderArrow;

    void Start()
    {

        // 画面に表示されない縦数を見つける
        UnderArrow = GameObject.Find("UnderArrow");

       // ステージのクリア条件
       StatusData = new StageStatus(3, 20);
        StatusData.StatusUpdate();

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


        btnFlg = ButtonFlg.NORMAL;

        EigoText = "";
        TransText = "";

        // 音声ファイルを設定
        soundTap = Resources.Load("SOUND/SE/cursor1", typeof(AudioClip)) as AudioClip;

    }

    // Update is called once per frame
    void Update()
    {
        // 画面に表示されない縦数を表示する
        UnderArrow.GetComponentInChildren<Text>().text = puzzleObjectGroup.ActiveBlockHeight.ToString();

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

            var button = EigoButton.GetComponent<Button>();

            //スターリワードをチェック
            StarData.StarCheck(EigoText);
            //            if (Input.GetMouseButtonDown(0))
            //            {

            StartCoroutine(BreakBlockCoroutine());

                if (isRunning == false)
                {

                    StatusData.HandScoreUpdate(EigoText);

                    //英語ボタンの文字を消す
                    EigoText = "";
                    EigoButton.GetComponentInChildren<Text>().text = EigoText;

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
                        blockData = collition2d.GetComponent<BlockData>();

                        if (blockData.blockType == BlockType.ALPHABET)
                        {
                            if (!blockData.Selected)
                            {
                                // プレイヤーがタップできるPuzzleDataのマスの高さ
                                if (blockData.Y >= puzzleObjectGroup.ActiveBlockHeight )
                                {
                                    audioSource = this.GetComponent<AudioSource>();
                                    audioSource.clip = soundTap;
                                    audioSource.Play();

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

            PuzzleDataList[i].GetComponent<SpriteRenderer>().sprite = null;
            PuzzleDataList[i].GetComponentInChildren<TextMesh>().GetComponent<EigoWordController>().breakFlg = true;
            yield return new WaitForSeconds(0.16f);

        }
        
        for (int i = 0; i < PuzzleDataList.Count(); i++)
        {
            PuzzleDataList[i].SetActive(false);
            //PuzzleDataList[i].GetComponentInChildren<Text>().fontSize = 80;
            //yield return new WaitForSeconds(0.25f);
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

            for (int i = 0; i < PuzzleDataList.Count(); i++)
            {
                PuzzleDataList[i].GetComponentInChildren<TextMesh>().GetComponent<EigoWordController>().scalingFlg = true;
            }
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

            for (int i = 0; i < PuzzleDataList.Count(); i++)
            {
                PuzzleDataList[i].GetComponentInChildren<TextMesh>().GetComponent<EigoWordController>().scalingFlg = false;
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

            // 和訳表示処理に移行
            GameFlg = GameLoopFlg.Translate;

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

}