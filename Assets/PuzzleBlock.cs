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

// ブロックのタイプを定義
public enum BlockType
{
    INVALID = -1,
    ALPHABET,
    BLOCK
}

public class BlockData
{

    //public Sprite[]BlockSprites;
    public Sprite BlockSprite;

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

    public BlockType GetType()
    {
        return this.blockType;
    }
    // コンストラクタでインスタンスを生成した時に情報を渡す
    public BlockData(BlockType type, string alphabet, bool selected, int x, int y, Sprite sprite)
    {
        this.blockType = type;
        this.Alphabet = alphabet;
        this.Selected = selected;
        this.X = x;
        this.Y = y;
        this.BlockSprite = sprite;

    }

    void Start()
    {
       // this.BlockSprite = BlockSprites[0];
        //rend = GetComponent<SpriteRenderer>();
        //rend.sprite = block[1];
        //this.transform.position = new Vector3(0, 0,0);

    }

    /*
    public PuzzleBlock Generate()
    {
        //var blockType = Random.Range(0, prefabs.Count);

        //GameObject instance = GameObject.Instantiate(block[0]);
        //instance.transform.SetParent(transform);
        //instance.SetChainLine(chainLineGroup.Get());
        //instance.SetBlockType(blockType);
        //instance.transform.position = new Vector3(UnityEngine.Random.Range(-300, 300), 600);
       //gidbodyList.Add(instance.GetComponent<Rigidbody2D>());
        //return instance;
    }
    */
}
