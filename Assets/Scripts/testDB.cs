﻿using System;
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

public class testDB : MonoBehaviour {

	// Use this for initialization
	void Start () {

        SqliteDatabase sqlDB = new SqliteDatabase("ejdict.sqlite3");

        string test = "apple";
        string query = "select mean from items where word ='"+test+"'";
        DataTable dataTable = sqlDB.ExecuteQuery(query);

        foreach (DataRow dr in dataTable.Rows)
        {
            string str = (string)dr["mean"];
           // attack = (int)dr["attack"];
            Debug.Log("name:" + str );
        }

        /*
        SqliteDatabase sqlDB = new SqliteDatabase("GameMaster.db");
        string query = "select name,attack from Weapon where id=2";
        DataTable dataTable = sqlDB.ExecuteQuery(query);

        string name = "";
        int attack = 0;
        foreach (DataRow dr in dataTable.Rows)
        {
            name = (string)dr["name"];
            attack = (int)dr["attack"];
            Debug.Log("name:" + name + " attack:" + attack);
        }
        */

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
