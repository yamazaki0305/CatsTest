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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //他のオブジェクトと接触した場合の処理
    void OnCollisionEnter2D(Collision2D other)
    {
        //Debug.Log("当たり判定");
        //ブロックに衝突した場合
        if (other.gameObject.tag == "Block")
        {
            if (other.gameObject.GetComponent<BlockData>().blockType == BlockType.CAT)
            {
                GameObject obj = GameObject.Find("RootCanvas");
                obj.GetComponent<PuzzleMain>().StatusData.Cat--;
                obj.GetComponent<PuzzleMain>().StatusUpdate();
                Debug.Log("当たり判定");
                Destroy(other.gameObject);
            }
        }

    }
}
