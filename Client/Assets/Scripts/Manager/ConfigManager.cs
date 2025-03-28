using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cfg;
using SimpleJSON;
using System.IO;

public static class ConfigManager
{
    private static Tables tables;
#if UNITY_EDITOR
    private static string jsonPath = "Assets/Scripts/GameConfig/Json";
#else
    private static string jsonPath = Application.streamingAssetsPath;
#endif

    public static IEnumerator Init()
    {
        if (tables != null)
            yield break;

        tables = new Tables(LoadJson);
    }

    public static PropCfg GetPropCfg(int id)
    {
        return tables.PropTable.GetOrDefault(id);
    }

    private static JSONNode LoadJson(string file)
    {
        return JSON.Parse(File.ReadAllText($"{jsonPath}/{file}.json", System.Text.Encoding.UTF8));
    }
}
