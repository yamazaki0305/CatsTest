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

public class DestroyCat : MonoBehaviour {

    public float fadeTime = 1f;

    private float currentRemainTime;
    private SpriteRenderer spRenderer;

    // リストを作っている
    private List<BlockData> blockDataList = new List<BlockData>();

    // Use this for initialization
    void Start () {

        // 初期化
        currentRemainTime = Time.deltaTime;
        spRenderer = GetComponent<SpriteRenderer>();

    }
	
	// Update is called once per frame
	void Update () {
        // 残り時間を更新
        currentRemainTime -= Time.deltaTime;
       // Debug.Log("Time:"+currentRemainTime);

    }

    //他のオブジェクトと接触した場合の処理
    void OnCollisionEnter2D(Collision2D other)
    //void OnTriggerStay2D(Collider2D other)
    {
        //ブロックに衝突した場合
        if (other.gameObject.tag == "Block")
        {
            GameObject objcol = other.gameObject;

            if (objcol.GetComponent<BlockData>().blockType == BlockType.CAT)
            {
                objcol.GetComponent<BlockData>().death = true;

                /*
                if (objcol.GetComponent<BlockData>().alpha == 1f )
                {
                    currentRemainTime = fadeTime;
                    currentRemainTime = Time.deltaTime;
                    objcol.GetComponent<BlockData>().alpha = 0.99f;

                }
                else if (objcol.GetComponent<BlockData>().alpha <= 0f)
                {
                    GameObject obj = GameObject.Find("RootCanvas");
                    obj.GetComponent<PuzzleMain>().StatusData.Cat--;
                    obj.GetComponent<PuzzleMain>().StatusUpdate();

                    // 残り時間が無くなったら自分自身を消滅
                    GameObject.Destroy(objcol);
                    return;
                }
                */
            }
        }
    }

        
                // フェードアウト
                /*
                objcol.GetComponent<BlockData>().alpha -= Time.deltaTime/ fadeTime;

                var color = objcol.GetComponent<SpriteRenderer>().color;
                color.a = objcol.GetComponent<BlockData>().alpha;
                objcol.GetComponent<SpriteRenderer>().color = color;
                */

}
