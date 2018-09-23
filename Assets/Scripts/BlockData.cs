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
    ALPHABET = 0,
    CRIMP,
    CAT = 4,
    SPACE
}

public class BlockData : MonoBehaviour
{
     
    //public Sprite[]BlockSprites;
    public Sprite[] Sprites;

    // 並んでいるブロックのタイプを定義する。
    public BlockType blockType = BlockType.INVALID;

    // ブロックのアルファベットを格納する。
    public string Alphabet;

    // 選択されているか？
    public bool Selected;

    //英単語か？
    public bool EigoFlg;

    // 7かける7のX座標
    public int X;

    // 7かける7のY座標
    public int Y;


    public void TapBlock()
    {
        if (this.Selected == false)
        {
            this.Selected = true;
            if (this.blockType == BlockType.ALPHABET)
                this.GetComponent<SpriteRenderer>().sprite = Sprites[1];
        }
        //this.GetComponentInChildren<TextMesh>().text = this.Alphabet;

    }
    public void ChangeBlock(bool selected,bool eigoflg )
    {
        
        if (selected == true && eigoflg == true)
        {
            this.Selected = selected;
            this.EigoFlg = eigoflg;
            this.GetComponent<SpriteRenderer>().sprite = Sprites[2];
        }
        else if (selected == true && eigoflg == false)
        {
            this.Selected = selected;
            this.EigoFlg = eigoflg;
            this.GetComponent<SpriteRenderer>().sprite = Sprites[1];
        }
        else if (selected == false && eigoflg == false)
        {
            this.Selected = selected;
            this.EigoFlg = eigoflg;
            this.GetComponent<SpriteRenderer>().sprite = Sprites[0];
        }

    }

    public void setup(BlockType type, string alphabet, bool selected, int x, int y)
    {
        this.blockType = type;
        this.Alphabet = alphabet;
        this.Selected = selected;
        this.EigoFlg = false;
        this.X = x;
        this.Y = y;

        if(this.blockType==BlockType.ALPHABET)
        {
            this.GetComponent<SpriteRenderer>().sprite = Sprites[(int)BlockType.ALPHABET];
            this.GetComponentInChildren<TextMesh>().text = this.Alphabet;
        }
        else if(this.blockType==BlockType.CAT)
        {
            this.GetComponent<SpriteRenderer>().sprite = Sprites[(int)BlockType.CAT];
            this.GetComponentInChildren<TextMesh>().text = "";
        }


    }
    /*
    // コンストラクタでインスタンスを生成した時に情報を渡す
    public BlockData(BlockType type, string alphabet, bool selected, int x, int y)
    {
        this.blockType = type;
        this.Alphabet = alphabet;
        this.Selected = selected;
        this.X = x;
        this.Y = y;
        
    }
    */

    void Start()
    {
        

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
