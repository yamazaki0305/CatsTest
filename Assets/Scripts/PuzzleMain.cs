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


// Macで書く
public class PuzzleMain : MonoBehaviour
{
    /// <summary>
    /// PuzzleObjectGroupからコピー
    /// </summary>

    //英語ブロックのサイズを指定
    private int BlockSize = 85;
    private int MaskSize = 90;

    //英語ブロックの1段目の高さ
    private int BlockGroundHeight = -250;

    //草ラインの高さ
    private int GlassLineHeight = -280;

    //デフォルトの高さのマス
    private int DefaultBlockHeight = 7;

    //地面に到着して猫を消すマスの高さ
    public int DeathBlockHeight = 0;

    //プレイヤーがタップ出来るPuzzleDataのマスの高さ
    private int ActiveBlockHeight = 0;

    //UnderArrowの残りブロック数
    private int UnderArrowHeight = 0;

    // PuzzleDataをPuzzleObjectGroup下に表示する
    public Transform puzzleTransform;

    //public Sprite[] puzzleSprites;

    public GameObject puzzlePrefab;
    public GameObject MaskPrefab;

    // 7列のパズルデータを作成。このパズルのデータでゲームを制御
    public GameObject[,] PuzzleData;

    // 7列のパズルデータのエリア内を作成（Mask）
    public GameObject[,] MaskData;

    public string[,] stageData;

    public string[] textMessage; //テキストの加工前の一行を入れる変数
    public string[,] textWords; //テキストの複数列を入れる2次元は配列
    private int rowLength; //テキスト内の行数を取得する変数
    private int columnLength; //テキスト内の列数を取得する変数

    private int[] can_alphabet = new int[26]; //A-Zまでパズルエリアに何文字あるか格納する
    private Text CanWordText; // 何単語作れるかのテキストを表示

    //////////////////////////////////////////////////////////// 

    // SEを所得
    private AudioClip soundTap;
    private AudioClip soundCancel;
    private AudioSource audioSource;

    public GameObject EigoButton;

    //ヘッダーStatusのアタッチ
    public GameObject StatusCat;
    public GameObject StatusHand;
    public GameObject StatusScore;

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
    GameLoopFlg GameFlg = GameLoopFlg.StartInfo;

    bool isRunning = true;

    // 画面に表示されないブロックの縦数を表示するGameObject
    private GameObject UnderArrow;

    // 猫救出ラインのGlassLineを表示するGameObject
    private GameObject GlassLine;

    private GameObject BackPicture;


    void Start()
    {

        /// <summary>
        /// PuzzleObjectGroupからコピー
        /// </summary>
        stageMaker("stage2");
        ActiveBlockHeight = rowLength - DefaultBlockHeight;
        UnderArrowHeight = ActiveBlockHeight;

        // can_alphabetにパズルエリアのアルフェベットを格納
        CheckPotentialPuzzle();
        CanWordText = GameObject.Find("CanWordText").GetComponent<Text>();
        CanWordText.text = "";
        /////////////////

        // 画面に表示されない縦数を見つける
        UnderArrow = GameObject.Find("UnderArrow");

        // 猫救出ラインのGlassLineを見つける
        GlassLine = GameObject.Find("GlassLine");
        Vector3 pos = new Vector3(0, GlassLineHeight + UnderArrowHeight  * -BlockSize, -1);
        GlassLine.transform.localPosition = pos;

        BackPicture = GameObject.Find("BackPicture");
        pos = new Vector3(0, GlassLineHeight + UnderArrowHeight * -BlockSize +500, 100);
        BackPicture.transform.localPosition = pos;

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
        StarData = new StarReword("ANIMAL", "[名]動物", "PEOPLE", "[名]人々", "CHECK", "[動]確認する");

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
        if (UnderArrowHeight == 0)
        {
            UnderArrow.SetActive(false);
        }
        else
        {
            // 画面に表示されない縦数を表示する
            UnderArrow.GetComponentInChildren<Text>().text = UnderArrowHeight.ToString();
        }

        // ゲーム開始前処理
        if (GameFlg == GameLoopFlg.StartInfo)
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

            CanWordText.GetComponent<Text>().text = "";

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

                SelectEigoDestroy();

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
            if (CheckBlockMove() == false)
            {
                // 和訳の表示をしない
                TransWindow.SetActive(false);

                if (DeathCat() == false)
                {
                    GameFlg = GameLoopFlg.UndderArrow;
                }
                /*
                audioSource = this.GetComponent<AudioSource>();
                audioSource.clip = soundStar;
                audioSource.Play();
                */
            }
        }
        // 地面の下にブロックがある時の処理
        else if (GameFlg == GameLoopFlg.UndderArrow)
        {
            while (UndderArrowCheck())
            {

            }

            GameFlg = GameLoopFlg.PlayBefore;

        }
        // ゲーム中処理に戻る前の処理
        else if (GameFlg == GameLoopFlg.PlayBefore)
        {
            // can_alphabetにパズルエリアのアルフェベットを格納
            CheckPotentialPuzzle();

            GameFlg = GameLoopFlg.PlayNow;
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
                                if (blockData.Y >= ActiveBlockHeight)
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
                                        SelectEigoChange();


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

    IEnumerator BreakBlockCoroutine()
    {

        for (int i = 0; i < PuzzleDataList.Count(); i++)
        {

            PuzzleDataList[i].GetComponent<SpriteRenderer>().sprite = null;
            PuzzleDataList[i].GetComponentInChildren<TextMesh>().GetComponent<EigoWordController>().breakFlg = true;
            yield return new WaitForSeconds(0.2f);

        }

        for (int i = 0; i < PuzzleDataList.Count(); i++)
        {
            PuzzleDataList[i].SetActive(false);
            //PuzzleDataList[i].GetComponentInChildren<Text>().fontSize = 80;
            //yield return new WaitForSeconds(0.25f);
        }
        isRunning = false;
    }

    IEnumerator sleep(float f)
    {

        yield return new WaitForSeconds(f);

    }
    bool EigoJudgement()
    {
        // 英単語が作れるかチェック
        CheckPotentialWords(EigoText);

        //英単語になったかの判定(2文字以上の時)
        bool judge = false;
        //英単語になったときの単語
        string eigoword = "temp";

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

                TransText += str;

            }

            if (judge == false)
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

                    eigoword = word;

                    TransText += str;

                }
            }
            if (judge == false)
            {
                //全て大文字にする
                string inStr = EigoText.ToUpperInvariant();
                
                query = "select word,mean from items where word ='" + inStr + "'";
                dataTable = sqlDB.ExecuteQuery(query);
                TransText = "";
                foreach (DataRow dr in dataTable.Rows)
                {
                    judge = true;
                    string word = (string)dr["word"];
                    string str = (string)dr["mean"];

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
            SelectEigoChange();
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
        CanWordText.text = "";

        if (btnFlg == ButtonFlg.PRESSED)
        {
            audioSource = this.GetComponent<AudioSource>();
            audioSource.clip = soundCancel;
            audioSource.Play();

            EigoText = "";
            EigoButton.GetComponentInChildren<Text>().text = EigoText;
            SelectAllCanceled();
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

    // 手数を消費してアルファベットブロックを落とす
    public void DropBlockButton()
    {
        bool search = true;



        // Maskと英語ブロックを参照して空きスペースを探す
        for (int j = ActiveBlockHeight - UnderArrowHeight; j < rowLength; j++)
        {
            // Maskと英語ブロックを参照して空きスペースを探す
            for (int i = 0; i < columnLength; i++)
            {
                //PuzzleDataが空白の時
                if (PuzzleData[i, j] == null && MaskData[i, j] != null)
                {

                    search = false;

                    // パズルを落とす位置をセット
                    Vector2 pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, columnLength * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize);

                    // スクリプトからインスタンス（動的にゲームオブジェクトを指定数だけ作る
                    PuzzleData[i, j] = Instantiate(puzzlePrefab, pos, Quaternion.identity);

                    // アルファベットブロックにする
                    PuzzleData[i, j].GetComponent<BlockData>().setup(BlockType.ALPHABET, RandomMake.alphabet(), false, i, j);
                    PuzzleData[i, j].name = "Block"; // GameObjectの名前を決めている
                    PuzzleData[i, j].transform.SetParent(puzzleTransform);
                    PuzzleData[i, j].transform.localPosition = pos;

                    //アルフェベットブロックの位置をセット
                    pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, j * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize);
                    PuzzleData[i, j].GetComponent<Liner>().OnUpper(pos, columnLength-j+1);
                    //PuzzleData[i, j].GetComponent<Liner>().OnStart(pos, k);
                    //PuzzleData[i, j].transform.position = pos;
                    //PuzzleData[i, j].transform.localScale = puzzlePrefab.transform.localScale;

                    //アルファベットブロックを落としたので手数を１つ減らす
                    StatusData.Hand--;
                    StatusData.StatusUpdate();

                    //パズルエリアの英語ブロック数を更新
                    CheckPotentialPuzzle();

                }

                if (!search)
                    break;
            }
            if (!search)
                break;
        }


        // ActiveBlockHeightから上DefaultBlockHeight(7段)までPuzzleData[,]を参照して空き配列を見る
        // 空き配列にMaskData[,]を確認しMaskが存在する
        //アルファベットブロックを落とす

        //空きスペースがない returnする
        return;
        //
        Debug.Log("DropBlock");

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

    /// <summary>
    /// PuzzleObjectGroupからコピー
    /// </summary>

    public bool DeathCat()
    {
        bool b = false;
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {
                //空白の時
                if (PuzzleData[i, j])
                {
                    if (PuzzleData[i, j].GetComponent<BlockData>().blockType == BlockType.CAT)
                    {
                        if (PuzzleData[i, j].GetComponent<BlockData>().death && PuzzleData[i, j].GetComponent<Liner>().iMove == false)
                        {
                            //消せる猫がいる時はreturn trueにする
                            b = true;

                            // 救出処理開始時
                            if (PuzzleData[i, j].GetComponent<BlockData>().alpha == 1.0f)
                            {
                                AudioSource a1;
                                AudioClip audio = Resources.Load("SOUND/SE/cat1", typeof(AudioClip)) as AudioClip;
                                a1 = gameObject.AddComponent<AudioSource>();
                                a1.clip = audio;
                                a1.Play();
                            }

                            PuzzleData[i, j].GetComponent<BlockData>().alpha -= 0.01f;
                            var color = PuzzleData[i, j].GetComponent<SpriteRenderer>().color;
                            color.a = PuzzleData[i, j].GetComponent<BlockData>().alpha;
                            PuzzleData[i, j].GetComponent<SpriteRenderer>().color = color;

                            if (PuzzleData[i, j].GetComponent<BlockData>().alpha < 0)
                            {
                                GameObject obj = GameObject.Find("GameRoot");

                                obj.GetComponent<PuzzleMain>().StatusData.CatUpdate();

                                // 残り時間が無くなったら自分自身を消滅
                                GameObject.Destroy(PuzzleData[i, j]);

                            }
                        }

                    }
                }

            }
        }
        return b;
    }


    //現在選択中のブロックを全てキャンセル
    public void SelectAllCanceled()
    {
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {
                if (PuzzleData[i, j] != null)
                {
                    if (PuzzleData[i, j].GetComponent<BlockData>().Selected)
                    {
                        PuzzleData[i, j].GetComponent<BlockData>().ChangeBlock(false, false);
                        Vector2 pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, j * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize);
                        PuzzleData[i, j].transform.SetParent(puzzleTransform);
                        //PuzzleData[i, j].transform.position = pos;
                    }

                }

            }
        }
    }

    //現在選択中のブロックを英単語にする
    public void SelectEigoChange()
    {
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {
                if (PuzzleData[i, j] != null)
                {
                    if (PuzzleData[i, j].GetComponent<BlockData>().Selected)
                    {

                        PuzzleData[i, j].GetComponent<BlockData>().ChangeBlock(true, true);
                        Vector2 pos = new Vector2(i * BlockSize - 320 + 45, j * BlockSize - 270);
                        PuzzleData[i, j].transform.SetParent(puzzleTransform);
                        //PuzzleData[i, j].transform.position = pos;
                    }

                }

            }
        }
    }
    //現在選択中の英語ブロックを消す
    public void SelectEigoDestroy()
    {
        List<BlockData> blockDataList = new List<BlockData>();


        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {
                if (PuzzleData[i, j] != null)
                {
                    if (PuzzleData[i, j].GetComponent<BlockData>().EigoFlg)
                    {
                        blockDataList.Add(PuzzleData[i, j].GetComponent<BlockData>());
                        Destroy(PuzzleData[i, j]);
                        PuzzleData[i, j] = null;
                        //yield return new WaitForSeconds(0.2f);
                    }

                }

            }
        }


        //PuzzleDataの空白を探す
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = ActiveBlockHeight - UnderArrowHeight; j < rowLength; j++)
            {
                //PuzzleDataが空白の時
                if (PuzzleData[i, j] == null && MaskData[i, j] != null)
                {

                    //空白PuzzleDataのブロックの上にブロックがないかrowLengthまで調べる
                    for (int k = 1; j + k < rowLength; k++)
                    {

                        //もしNULL以外のPuzzleDataのブロックが見つかった時
                        if (PuzzleData[i, j + k] != null)
                        {
                            //PuzzleDataのX,Yのマスの位置を現在→新しい位置に更新
                            PuzzleData[i, j + k].GetComponent<BlockData>().X = i;
                            PuzzleData[i, j + k].GetComponent<BlockData>().Y = j;

                            //空白PuzzleDataの空白に見つかったPuzzleDataのブロックを代入
                            PuzzleData[i, j] = PuzzleData[i, j + k];

                            //空白のPuzzleDataに代入したので代入元のデータをnullにする
                            PuzzleData[i, j + k] = null;

                            //PuzzleDataのブロックの表示座標を更新する
                            //Vector2 pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, j * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize + (ActiveBlockHeight - UnderArrowHeight) * BlockSize);
                            Vector2 pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, j * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize);
                            PuzzleData[i, j].transform.SetParent(puzzleTransform);

                            //PuzzleData[i, j].GetComponent<Liner>().OnMove(pos, k);
                            PuzzleData[i, j].GetComponent<Liner>().OnStart(pos, k);
                            //PuzzleData[i, j].transform.position = pos;
                            PuzzleData[i, j].transform.localScale = puzzlePrefab.transform.localScale;

                            // 空白PuzzleDataのブロックの上にブロックがないかrowLengthまで調べるのを終了
                            k = 100;

                        }
                    }
                }
            }
        }



        //地面に到着した猫を探す
        for (int i = 0; i < columnLength; i++)
        {
            if (PuzzleData[i, DeathBlockHeight] != null)
            {
                if (PuzzleData[i, DeathBlockHeight].GetComponent<BlockData>().blockType == BlockType.CAT)
                {
                    PuzzleData[i, DeathBlockHeight].GetComponent<BlockData>().death = true;

                }
            }
        }
    }

    // 移動中のブロックがないかチェック true:移動中、false:移動中なし
    public bool CheckBlockMove()
    {
        //PuzzleDataが移動中か調べる
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {
                //空白のPuzzleData以外の時
                if (PuzzleData[i, j] != null)
                {
                    //PuzzleDataが移動中の時
                    if (PuzzleData[i, j].GetComponent<Liner>().iMove == true)
                    {
                        return true;
                    }
                }

            }
        }
        return false;

    }

    // ステージのブロックを作成
    public void stageMaker(string filename)
    {

        //　テキストファイルからデータを読み込む
        TextAsset textasset = new TextAsset(); //テキストファイルのデータを取得するインスタンスを作成
        textasset = Resources.Load(filename, typeof(TextAsset)) as TextAsset; //Resourcesフォルダから対象テキストを取得
        string TextLines = textasset.text; //テキスト全体をstring型で入れる変数を用意して入れる

        //Splitで一行づつを代入した1次配列を作成
        textMessage = TextLines.Split('\n'); //

        //行数と列数を取得
        string[] columstr = textMessage[0].Split(',');
        columnLength = columstr.Length - 1;
        rowLength = textMessage.Length;

        // 画面に出すブロックの縦数は最大7にする
        if (rowLength > 7)
            DefaultBlockHeight = 7;
        else
            DefaultBlockHeight = rowLength;

        // ステージ用のテキストファイルを２次元配列データに格納する用の２次元配列を作成
        stageData = new string[columnLength, rowLength];

        // stageDataから空き枠以外をMaskDataに格納する用の２次元配列を作成
        MaskData = new GameObject[columnLength, rowLength+1];

        // stageDataから英語ブロック、猫などを格納する用の２次元配列を作成 
        PuzzleData = new GameObject[columnLength, rowLength];


        //2次配列を定義
        textWords = new string[rowLength, columnLength];

        for (int i = 0; i < rowLength; i++)
        {

            string[] tempWords = textMessage[i].Split(','); //textMessageをカンマごとに分けたものを一時的にtempWordsに代入

            for (int n = 0; n < columnLength; n++)
            {
                textWords[i, n] = tempWords[n]; //2次配列textWordsにカンマごとに分けたtempWordsを代入していく
            }
        }

        char[] eigochar = "AAAAAABBCCCDDDEEEEEEFFGGGHHHIIIIIJKKKLLLMMMNNNOOOOPPQRRRSSSTTTUUUUVWWXYYYZ".ToCharArray();

        int k = rowLength - 1;
        for (int i = 0; i < rowLength; i++)
        {
            for (int n = 0; n < columnLength; n++)
            {
                string str = textWords[i, n];

                //neko
                if (str == "*")
                {
                    stageData[n, k] = "cat";
                }
                //maskなし
                else if (str == "-")
                {
                    stageData[n, k] = "";
                }
                //アルファベットランダム
                else if (str == "#")
                {
                    int rand = UnityEngine.Random.Range(0, eigochar.Length);
                    stageData[n, k] = eigochar[rand].ToString();
                }
                //アルファベット
                else
                {
                    stageData[n, k] = str;
                }

            }
            k--;
        }

        // stageDataからMaskDataを作成する
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {

                //空白の時
                if (stageData[i, j] != "")
                {

                    Vector2 pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, j * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize);

                    // スクリプトからインスタンス（動的にゲームオブジェクトを指定数だけ作る
                    MaskData[i, j] = Instantiate(MaskPrefab, pos, Quaternion.identity);

                    MaskData[i, j].name = "Mask";
                    MaskData[i, j].transform.SetParent(puzzleTransform);
                    MaskData[i, j].transform.localPosition = pos;
                    MaskData[i, j].transform.localScale = MaskPrefab.transform.localScale;

                }
            }
        }

        // stageDataからPuzzleDataを作成する
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {

                //空白の時
                if (stageData[i, j] != "")
                {

                    Vector2 pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, j * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize);

                    //Vector2 pos = new Vector2(i * BlockSize - 320 + 45 + margin, j * BlockSize - 270);

                    // スクリプトからインスタンス（動的にゲームオブジェクトを指定数だけ作る
                    PuzzleData[i, j] = Instantiate(puzzlePrefab, pos, Quaternion.identity);
                    if (stageData[i, j] == "cat")
                    {
                        PuzzleData[i, j].GetComponent<BlockData>().setup(BlockType.CAT, stageData[i, j], false, i, j);
                        PuzzleData[i, j].name = "Cat"; // GameObjectの名前を決めている

                    }
                    else
                    {
                        PuzzleData[i, j].GetComponent<BlockData>().setup(BlockType.ALPHABET, stageData[i, j], false, i, j);
                        PuzzleData[i, j].name = "Block"; // GameObjectの名前を決めている
                    }

                    // 生成したGameObjectをヒエラルキーに表示
                    PuzzleData[i, j].transform.SetParent(puzzleTransform);
                    PuzzleData[i, j].transform.localPosition = pos;
                    PuzzleData[i, j].transform.localScale = puzzlePrefab.transform.localScale;

                }
            }
        }


    }

    /// <returns></returns>

    public bool UndderArrowCheck()
    {
        // 地面の下にブロックがある時
        if (UnderArrowHeight > 0)
        {
            for (int i = 0; i < columnLength; i++)
            {
                // 最上部行が全て空いているか確認（Maskのないエリアの場合も空いているカウントする）
                if (PuzzleData[i, rowLength - 1] != null)
                {
                    BlockData blockData = PuzzleData[i, rowLength - 1].GetComponent<BlockData>();

                    //詰まっている列があれば何もしない
                    if (blockData.blockType == BlockType.ALPHABET || blockData.blockType == BlockType.CAT)
                    {
                        return false;
                    }
                }
            }

            //上に移動できる分全ての列を一番高い列が詰まるまで移動
            for (int i = 0; i < columnLength; i++)
            {
                for (int j = rowLength - 2; j >= 0; j--)
                {
                    //もしNULL以外のPuzzleDataのブロックが見つかった時
                    if (PuzzleData[i, j] != null)
                    {
                        //PuzzleData[i, j].GetComponent<BlockData>().X = i;
                        PuzzleData[i, j].GetComponent<BlockData>().Y = j + 1;

                        PuzzleData[i, j + 1] = PuzzleData[i, j];
                        PuzzleData[i, j] = null;

                        //PuzzleDataのブロックの表示座標を更新する
                        Vector2 pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, (j + 1) * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize);

                        PuzzleData[i, j + 1].transform.SetParent(puzzleTransform);

                        PuzzleData[i, j + 1].GetComponent<Liner>().OnUpper(pos, 1);
                        PuzzleData[i, j + 1].transform.localScale = puzzlePrefab.transform.localScale;
                    }

                }
            }

            for (int i = 0; i < columnLength; i++)
            {
                for (int j = rowLength - 1; j >= 0; j--)
                {
                    //もしNULL以外のPuzzleDataのブロックが見つかった時
                    if (MaskData[i, j] != null)
                    {
                        //PuzzleData[i, j].GetComponent<BlockData>().X = i;
                        //PuzzleData[i, j].GetComponent<BlockData>().Y = j + 1;

                        MaskData[i, j + 1] = MaskData[i, j];
                        MaskData[i, j] = null;

                        //PuzzleDataのブロックの表示座標を更新する
                        Vector2 pos = new Vector2(i * BlockSize - (BlockSize * columnLength) / 2 + BlockSize / 2, (j + 1) * BlockSize + BlockGroundHeight - (rowLength - DefaultBlockHeight) * BlockSize);

                        MaskData[i, j + 1].transform.localScale = MaskPrefab.transform.localScale;
                        MaskData[i, j + 1].transform.SetParent(puzzleTransform);
                        MaskData[i, j + 1].GetComponent<Liner>().OnUpper(pos, 1);
                    }

                }
            }

            /*
            // MaskはPuzzleDataみたいにX,Yの座標は変えず、位置を上に移動させる
            for (int i = 0; i < columnLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    //空白の時
                    if (MaskData[i, j] == null)
                    {
                        if (j - 1 > 0)
                            if (MaskData[i, j - 1] != null)
                                MaskData[i, j] = new GameObject(); // 空のGameObjectを入れる
                    }
                    else
                    {
                        //PuzzleDataのブロックの表示座標を更新する
                        Vector2 pos = new Vector2(MaskData[i, j].transform.localPosition.x, MaskData[i, j].transform.localPosition.y + MaskSize);

                        MaskData[i, j].transform.SetParent(puzzleTransform);

                        MaskData[i, j].GetComponent<Liner>().OnUpper(pos, 1);
                        //MaskData[i, j].transform.localScale = puzzlePrefab.transform.localScale;
                    }

                }
            }
            */

            // 残り上の行に移動できる回数をへらす
            UnderArrowHeight--;

            // ブックを消すPuzzleDataの座標を1つ上げる
            DeathBlockHeight++;

            // GlassLineを移動する
            Vector3 glass_pos = new Vector3(0, GlassLineHeight + UnderArrowHeight * -BlockSize, -1);
            GlassLine.GetComponent<Liner>().OnUpper(glass_pos, 1);

            Vector3 back_pos = new Vector3(0, GlassLineHeight + UnderArrowHeight * -BlockSize + 500, 100);
            BackPicture.GetComponent<Liner>().OnUpper(back_pos, 1);

            //４列が上まで詰まっているか確認（Maskのないエリアの場合も最上部行があいていれば上に行く）
            // 最上部行が全て空いているか確認（Maskのないエリアの場合も空いているカウントする）
            //詰まっている列があれば何もしない、
            //詰まっていない場合は何個上に移動できるか計算（1段ごとの計算でできなくなるまでループさせる）
            //上に移動できる分全ての列を一番高い列が詰まるまで移動。
            //列の最大個数より小さい列は移動しない。 

            return true;
        }
        else
            return false;

    }

    // can_alphabetにパズルエリアのアルフェベットを格納
    public void CheckPotentialPuzzle()
    {
        char[] eigochar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        // パズルエリアのアルファベットA-Zまで0を格納する
        for (int i = 0; i < 26; i++)
        {
            can_alphabet[i] = 0;
        }

        // パズルエリアのアルファベットの数を格納する
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = ActiveBlockHeight; j < rowLength; j++)
            {

                if (PuzzleData[i, j] != null)
                {
                    for (int k = 0; k < 26; k++)
                    {
                        char eigo = PuzzleData[i, j].GetComponent<BlockData>().Alphabet[0];
                        if (eigochar[k] == eigo)
                        {
                            can_alphabet[k]++;
                            k = 100;
                        }
                    }

                }
            }
        }

        // 現在のアルファベットブロック数をDebugLogに表示
        for (int i = 0; i < 26; i++)
        {
            Debug.Log(eigochar[i] + ":" + can_alphabet[i]);
        }
    }


    //英単語が作れる可能性をチェック
    public void CheckPotentialWords(string eigostr)
    {
        char[] eigochar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        Text CanWordText = GameObject.Find("CanWordText").GetComponent<Text>();

        // 英単語が1文字以下の時はCanWordTextを表示しない
        if (eigostr.Length < 1)
        {
            CanWordText.GetComponent<Text>().text = "";
            return;
        }

        //英単語としてカウントするか
        bool judge = false;

        // SQLite3のSQL文を作成　現在の選択中の英単語+%で英単語の可能性のある単語をSelectする
        string query = "select word,mean from items where word like '"+eigostr+"%'";
        DataTable dataTable = sqlDB.ExecuteQuery(query);

        TransText = "";
        int moutch_count = 0; //何単語作れるか

        string lastword=null; // マッチした英語の前回の単語を可能

        // SQL文で見つかった英単語を1つ1つ調べて、judge=trueなら作れる英単語を足す
        foreach (DataRow dr in dataTable.Rows)
        {
            judge = true;

            string word = (string)dr["word"];
            string str = (string)dr["mean"];

            if (word.Length < 2)
                judge = false;

            if (judge)
            {

                // 前回と同じ英単語は除く（英語辞書データに同じ英単語がある場合があるための処理）
                if(lastword == word.ToUpperInvariant())
                {
                    judge = false;
                }
                else
                {
                    //除外リストの単語かチェック
                    for (int i = 0; i < ignore_word.Length; i++)
                    {
                        //もし除外リストの単語だったら
                        if (ignore_word[i].ToString() == word)
                        {
                            judge = false;
                            break;
                        }
                    }
                }
            }

            if (judge)
            {
                string tempword = word.ToUpperInvariant();


                // パズル画面中のアルファベット格納数can_alphabetをtemp_alphabetにコピー
                int[] temp_alphabet = new int[26];
                for (int i = 0; i < 26; i++)
                    temp_alphabet[i] = can_alphabet[i];

                // 英単語を1文字ずつアルファベットを調べてパズル画面のアルファベット格納数の配列から1つ減らす処理をする
                for (int i = 0; i < tempword.Length; i++)
                {
                    for (int j = 0; j < 26; j++)
                    {

                        if (tempword[i] == eigochar[j])
                        {
                            temp_alphabet[j]--;
                            //もしアルファベットの数がマイナスになったらパズル画面中のアルファベットが足りなくなるので、作れる可能性のある英単語ではないと判定
                            if (temp_alphabet[j] < 0)
                                judge = false;

                            break;
                        }
                        // 英語辞書データにスペースなど2単語の和訳があるのでそういうのは除外（作れる可能性のある英単語ではないと判定
                        else if (tempword[i] == ' ' || tempword[i] == '.' || tempword[i] == '/' || tempword[i] == '-' || tempword[i] == '\'' )
                        {
                            judge = false;
                            break;
                        }
                    }
                    if (judge == false)
                        break;
                }
            }

            // 上記処理を通過した英単語は作れる可能性のある英単語と判定する
            if (judge)
            {
                Debug.Log("英単語:" + word);
                lastword = word.ToUpperInvariant();
                moutch_count++;
            }

            // 作れる可能性のある英単語が多すぎる場合、処理が重くなるので最大50単語で打ち止めする（foreachからbreak）
            if (moutch_count > 50)
                break;
        }

        Debug.Log("マッチ数:" + moutch_count);


        // 作れる可能性のある英単語が多すぎる場合、処理が重くなるので最大50単語で打ち止めする
        if (moutch_count>50)
            CanWordText.GetComponent<Text>().text = "50単語以上" ;
        else if(moutch_count>0)
            CanWordText.GetComponent<Text>().text = moutch_count + "単語";
        else if(moutch_count==0)
            CanWordText.GetComponent<Text>().text = "";
    }
}