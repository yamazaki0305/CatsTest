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

public class PuzzleObjectGroup : MonoBehaviour {

    //ステージの縦、横大きさ
    //private int Hsize = 7;
    //private int Wsize = 7;

    // 余白のサイズ
    private int margin = 5;


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

    // Use this for initialization
    void Start () {

        stageMaker();

        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {

                //空白の時
                if (stageData[i, j] != "")
                {

                    Vector2 pos = new Vector2(i * 90 - 320 + 45 + margin, j * 90 - 270);

                    // スクリプトからインスタンス（動的にゲームオブジェクトを指定数だけ作る
                    MaskData[i, j] = Instantiate(MaskPrefab, pos, Quaternion.identity);

                    MaskData[i, j].name = "Mask";
                    MaskData[i, j].transform.SetParent(puzzleTransform);
					MaskData[i, j].transform.localPosition = pos;
                    MaskData[i, j].transform.localScale = MaskPrefab.transform.localScale;

                }
            }
        }

        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {

                //空白の時
                if (stageData[i, j] != "")
                { 

                    Vector2 pos = new Vector2(i * 90 - 320 + 45 + margin, j * 90 - 270);

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


    // Update is called once per frame
    void Update() {

    }

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
                        if (PuzzleData[i, j].GetComponent<BlockData>().death && PuzzleData[i, j].GetComponent<Liner>().iMove == false )
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
                    if( PuzzleData[i, j].GetComponent<BlockData>().Selected )
                    {
                        PuzzleData[i, j].GetComponent<BlockData>().ChangeBlock(false,false);
                        Vector2 pos = new Vector2(i * 90 - 320 + 45, j * 90 - 270);
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
                        
                        PuzzleData[i, j].GetComponent<BlockData>().ChangeBlock(true,true);
                        Vector2 pos = new Vector2(i * 90 - 320 + 45, j * 90 - 270);
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
            for (int j = 0; j < rowLength; j++)
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
                            Vector2 pos = new Vector2(i * 90 - 320 + 45 + margin, j * 90 - 270);

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
            if (PuzzleData[i, 0] != null)
            {
                if (PuzzleData[i, 0].GetComponent<BlockData>().blockType == BlockType.CAT)
                {
                    PuzzleData[i, 0].GetComponent<BlockData>().death = true;

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
        public void stageMaker()
    {



        //　テキストファイルから読み込んだデータ
        TextAsset textasset = new TextAsset(); //テキストファイルのデータを取得するインスタンスを作成
        textasset = Resources.Load("stage2", typeof(TextAsset)) as TextAsset; //Resourcesフォルダから対象テキストを取得
        string TextLines = textasset.text; //テキスト全体をstring型で入れる変数を用意して入れる

        //Splitで一行づつを代入した1次配列を作成
        textMessage = TextLines.Split('\n'); //

        //行数と列数を取得
        string[] columstr = textMessage[0].Split(',');
        columnLength = columstr.Length-1;
        rowLength = textMessage.Length;

        Debug.Log("rowLength:" + rowLength);
        Debug.Log("columnLength:" + columnLength);

        // 7列のデータを作成している。このブロックのデータでゲームを制御
        PuzzleData = new GameObject[columnLength, rowLength];

        // 7列のデータを作成している。このブロックのデータでゲームを制御
        MaskData = new GameObject[columnLength, rowLength];

        stageData = new string[columnLength, rowLength];

    //2次配列を定義
    textWords = new string[rowLength, columnLength];

        for (int i = 0; i < rowLength; i++)
        {

            string[] tempWords = textMessage[i].Split(','); //textMessageをカンマごとに分けたものを一時的にtempWordsに代入

            for (int n = 0; n < columnLength; n++)
            {
                textWords[i, n] = tempWords[n]; //2次配列textWordsにカンマごとに分けたtempWordsを代入していく
                Debug.Log(textWords[i, n]);
            }
        }

        char[]eigochar = "AAAAAABBCCCDDDEEEEEEFFGGGHHHIIIIIJKKKLLLMMMNNNOOOOPPQRRRSSSTTTUUUUVWWXYYYZ".ToCharArray(); ;

        int k = rowLength-1;
        for (int i = 0; i < rowLength; i++)
        {
            for (int n = 0; n < columnLength; n++)
            {
                string str = textWords[i, n];
                Debug.Log("x:"+n+"y"+i+"str:" + str);
                //neko
                if (str=="*")
                {
                    stageData[n, k] = "cat";
                }
                //maskなし
                else if (str == "-")
                {
                    stageData[n, k] = "";
                }
                //アルファベットランダム
                else if(str =="#")
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

    }

}
