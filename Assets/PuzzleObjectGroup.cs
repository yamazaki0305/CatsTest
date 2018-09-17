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

    public Transform puzzleGroup;
    //public Sprite[] puzzleSprites;

    public GameObject puzzlePrefab;

    // 7列のデータを作成している。このブロックのデータでゲームを制御
    public GameObject[,] blockData = new GameObject[7, 7];

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

                    // 生成した玉をグループ化
                    blockData[i, j].transform.SetParent(puzzleGroup);
                    blockData[i, j].transform.position = pos;
                    blockData[i, j].transform.localScale = puzzlePrefab.transform.localScale;


                }
            }
        }

    }


    // Update is called once per frame
    void Update () {

        // スマホのタッチと、PCのクリック判定
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collition2d = Physics2D.OverlapPoint(point);

            if (collition2d)
            {
                if (collition2d.tag == "Block")
                {
                    // ここでRayが当たったGameObjectを取得できる
                    Debug.Log(collition2d.gameObject.name);
                    Debug.Log(collition2d.gameObject.GetComponent<BlockData>().GetType());
                }
            }
        }

    }

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
