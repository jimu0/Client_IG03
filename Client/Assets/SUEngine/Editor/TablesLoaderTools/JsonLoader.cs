using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using cfg;
using SimpleJSON;

public class TablesTools : Editor
{

    [MenuItem("Tools/TableImporter/Load Developers Table")]
    public static void LoadDevelopersTable()
    {
        string gameConfDir = "output_Json"; // 替换为gen.bat中outputDataDir指向的目录
        Tables tables = new cfg.Tables(file => JSON.Parse(File.ReadAllText($"{gameConfDir}/{file}.json")));
        //cfg.Tables.Reward reward = tables.TbReward.Get(1001);
        // // 访问一个单例表
        // Console.WriteLine(tables.Name);
        // // 访问普通的 key-value 表
        // Console.WriteLine(tables.TbItem.Get(1).Name);
        // // 支持 operator []用法
        // //Console.WriteLine(tables.TbMail[1001].Desc);
    }
}
