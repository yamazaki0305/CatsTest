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

    public Transform puzzleGroup;
    public Sprite[] puzzleSprites;

    public GameObject puzzlePrefab;
    public GameObject [,]target = new GameObject[7, 7];

    // 7列のデータを作成している。このブロックのデータでゲームを制御
    public PuzzleBlock[,] blockData = new PuzzleBlock[7, 7];


    // Use this for initialization
    void Start () {
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                
                // タイプとアルファベットは固定にしています
                blockData[i, j] = new PuzzleBlock(BlockType.ALPHABET, "A", false, i, j, puzzleSprites[0]);

                Vector2 pos = new Vector2(i*90-320+45, j*90-270);

                // スクリプトからインスタンス（動的にゲームオブジェクトを指定数だけ作る
                GameObject ball = Instantiate(puzzlePrefab, pos, Quaternion.identity);

                // 生成した玉をグループ化
                ball.transform.SetParent(puzzleGroup);
                ball.transform.position = pos;
                ball.transform.localScale = puzzlePrefab.transform.localScale;

                int spriteId = UnityEngine.Random.Range(0, 5);
                ball.name = "Ball" + spriteId; // GameObjectの名前を決めている
                SpriteRenderer spriteObj = ball.GetComponent<SpriteRenderer>();
                spriteObj.sprite = blockData[i, j].BlockSprite;  // GameObjectの名前を決めている
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
                // ここでRayが当たったGameObjectを取得できる
                Debug.Log(collition2d.gameObject.name);
            }
        }

    }
}
