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
    private int Hsize = 7;
    private int Wsize = 7;
    public int test = 1;
    public Transform puzzleTransform;

    //public Sprite[] puzzleSprites;

    public GameObject puzzlePrefab;
    public GameObject MaskPrefab;

    // 7列のデータを作成している。このブロックのデータでゲームを制御
    public GameObject[,] blockData = new GameObject[7, 7];

    // 7列のデータを作成している。このブロックのデータでゲームを制御
    public GameObject[,] MaskData = new GameObject[7, 7];

    public string[,] stageData = new string[7, 7];

    // Use this for initialization
    void Start () {

        stageMaker();

        for (int i = 0; i < Wsize; i++)
        {
            for (int j = 0; j < Hsize; j++)
            {

                //空白の時
                if (stageData[i, j] != "")
                { 

                    Vector2 pos = new Vector2(i * 90 - 320 + 45, j * 90 - 270);

                    // スクリプトからインスタンス（動的にゲームオブジェクトを指定数だけ作る
                    MaskData[i, j] = Instantiate(MaskPrefab, pos, Quaternion.identity);
                    blockData[i, j] = Instantiate(puzzlePrefab, pos, Quaternion.identity);
                    if (stageData[i, j] == "cat")
                    {
                        blockData[i, j].GetComponent<BlockData>().setup(BlockType.CAT, stageData[i, j], false, i, j);
                        blockData[i, j].name = "Cat"; // GameObjectの名前を決めている

                    }
                    else
                    {
                        MaskData[i, j] = MaskPrefab;
                        MaskData[i, j].name = "Mask";
                        blockData[i, j].GetComponent<BlockData>().setup(BlockType.ALPHABET, stageData[i, j], false, i, j);
                        blockData[i, j].name = "Block"; // GameObjectの名前を決めている
                    }


                    //MaskData[i, j].transform.SetParent(puzzleTransform);
                    //MaskData[i, j].transform.position = pos;
                    //MaskData[i, j].transform.localScale = MaskPrefab.transform.localScale;

                    // 生成した玉をグループ化
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

    //現在選択中のブロックを全てキャンセル
    public void SelectAllCanceled()
    {
        for (int i = 0; i < Wsize; i++)
        {
            for (int j = 0; j < Hsize; j++)
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
        for (int i = 0; i < Wsize; i++)
        {
            for (int j = 0; j < Hsize; j++)
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
        for (int i = 0; i < Wsize; i++)
        {
            for (int j = 0; j < Hsize; j++)
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
    }
    // ステージのブロックを作成
    public void stageMaker()
    {
        string[] array;
        array = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        for (int i = 0; i < Wsize; i++)
        {
            for (int j = 0; j < Hsize; j++)
            {
                int rand = UnityEngine.Random.Range(0, 26);
                stageData[i, j] = array[rand];
            }
        }

        //7列目を空白にする
        for (int i = 0; i < Wsize; i++)
        {
            stageData[i, Hsize - 1] = "";
        }
        //7列目に猫を配置
        stageData[1,Hsize - 1] ="cat";
        stageData[3,Hsize - 1] ="cat";
        stageData[5,Hsize - 1] ="cat";

    }
}
