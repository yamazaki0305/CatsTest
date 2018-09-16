using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData
{

    // 並んでいるブロックのタイプを定義する。
    public BlockType blockType = BlockType.INVALID;

    // ブロックのアルファベットを格納する。
    public string Alphabet;

    // 選択されているか？
    public bool Selected;

    // 7かける7のX座標
    public int X;

    // 7かける7のY座標
    public int Y;

    // コンストラクタでインスタンスを生成した時に情報を渡す
    public BlockData(BlockType type, string alphabet, bool selected, int x, int y)
    {
        this.blockType = type;
        this.Alphabet = alphabet;
        this.Selected = selected;
    }
}

public class BlockCreator : MonoBehaviour
{

    // 7列のデータを作成している。このブロックのデータでゲームを制御
    public BlockData[,] blockData = new BlockData[7, 7];

    // Use this for initialization
    void Start()
    {

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                // タイプとアルファベットは固定にしています
                blockData[i, j] = new BlockData(BlockType.ALPHABET, "A", false, i, j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
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