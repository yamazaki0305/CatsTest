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

public class MaskPrefab : MonoBehaviour {

	// Use this for initialization
	void Start () {

        var color = this.GetComponent<SpriteRenderer>().color;
        color.a = 0.5f;
        this.GetComponent<SpriteRenderer>().color = color;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
