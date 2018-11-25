using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVReader : MonoBehaviour {

    TextAsset csvFile; // CSVファイル
    public int height; // CSVの行数
    List<string[]> csvDatas = new List<string[]>(); // CSVの中身を入れるリスト;

    void Start()
    {
        csvFile = Resources.Load("testCSV") as TextAsset; // Resouces下のCSV読み込み
        //csvFile = Resources.Load("stagedata") as TextAsset; // Resouces下のCSV読み込み
        StringReader reader = new StringReader(csvFile.text);

        // , で分割しつつ一行ずつ読み込み
        // リストに追加していく
        while (reader.Peek() > -1) // reader.Peaekが0になるまで繰り返す
        {
            string line = reader.ReadLine(); // 一行ずつ読み込み
            csvDatas.Add(line.Split(',')); // , 区切りでリストに追加
            height++; // 行数加算
        }

        // csvDatas[行][列]を指定して値を自由に取り出せる
        Debug.Log(csvDatas[0][1]); 

    }

    // 疑問
    // TextAssetはナニモン？
    // StringReaderはナニモン？
    // わざわざリストに入れてるけどTextAssetのままでは使えないの？

}
