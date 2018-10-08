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

    // 7列のデータを作成している。このブロックのデータでゲームを制御
    public GameObject[,] blockData;

    // 7列のデータを作成している。このブロックのデータでゲームを制御
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
                    MaskData[i, j].transform.position = pos;
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
                    blockData[i, j] = Instantiate(puzzlePrefab, pos, Quaternion.identity);
                    if (stageData[i, j] == "cat")
                    {
                        blockData[i, j].GetComponent<BlockData>().setup(BlockType.CAT, stageData[i, j], false, i, j);
                        blockData[i, j].name = "Cat"; // GameObjectの名前を決めている

                    }
                    else
                    {
                        blockData[i, j].GetComponent<BlockData>().setup(BlockType.ALPHABET, stageData[i, j], false, i, j);
                        blockData[i, j].name = "Block"; // GameObjectの名前を決めている
                    }

                    // 生成したGameObjectをヒエラルキーに表示
                    blockData[i, j].transform.SetParent(puzzleTransform);
                    blockData[i, j].transform.position = pos;
                    blockData[i, j].transform.localScale = puzzlePrefab.transform.localScale;



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
                if (blockData[i, j])
                {
                    if (blockData[i, j].GetComponent<BlockData>().blockType == BlockType.CAT)
                    {
                        if (blockData[i, j].GetComponent<BlockData>().death)
                        {
                            blockData[i, j].GetComponent<BlockData>().alpha -= 0.02f;
                            var color = blockData[i, j].GetComponent<SpriteRenderer>().color;
                            color.a = blockData[i, j].GetComponent<BlockData>().alpha;
                            blockData[i, j].GetComponent<SpriteRenderer>().color = color;

                            if (blockData[i, j].GetComponent<BlockData>().alpha < 0)
                            {
                                GameObject obj = GameObject.Find("RootCanvas");
                                obj.GetComponent<PuzzleMain>().StatusData.Cat--;
                                obj.GetComponent<PuzzleMain>().StatusUpdate();

                                // 残り時間が無くなったら自分自身を消滅
                                GameObject.Destroy(blockData[i, j]);
                                b = true;
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
                if (blockData[i, j] != null)
                {
                    if( blockData[i, j].GetComponent<BlockData>().Selected )
                    {
                        blockData[i, j].GetComponent<BlockData>().ChangeBlock(false,false);
                        Vector2 pos = new Vector2(i * 90 - 320 + 45, j * 90 - 270);
                        blockData[i, j].transform.SetParent(puzzleTransform);
                        //blockData[i, j].transform.position = pos;
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
                if (blockData[i, j] != null)
                {
                    if (blockData[i, j].GetComponent<BlockData>().Selected)
                    {
                        
                        blockData[i, j].GetComponent<BlockData>().ChangeBlock(true,true);
                        Vector2 pos = new Vector2(i * 90 - 320 + 45, j * 90 - 270);
                        blockData[i, j].transform.SetParent(puzzleTransform);
                        //blockData[i, j].transform.position = pos;
                    }

                }

            }
        }
    }
    //現在選択中の英語ブロックを消す
    public void SelectEigoDestroy()
    {
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {
                if (blockData[i, j] != null)
                {
                    if (blockData[i, j].GetComponent<BlockData>().EigoFlg)
                    {
                        Destroy(blockData[i, j]);
                        
                    }

                }

            }
        }

        /*
        for (int i = 0; i < columnLength; i++)
        {
            for (int j = 0; j < rowLength; j++)
            {
                if (blockData[i, j] != null)
                {
                    if (blockData[i, j].GetComponent<BlockData>().EigoFlg)
                    {
                        Destroy(blockData[i, j]);

                    }

                }

            }
        }
        */
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
        blockData = new GameObject[columnLength, rowLength];

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
