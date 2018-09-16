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

public class BlockSprite: MonoBehaviour {

    public Sprite[] block;
    private SpriteRenderer rend;

	// Use this for initialization
	void Start () {
        rend = GetComponent<SpriteRenderer>();
        rend.sprite = block[1];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
