using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum EOperation
{
    Equals,
    NotEquals,
    Less,
    Lagger,
}

[Serializable]
public struct KVList
{
    public List<KeyValue> list;
}

[Serializable]
public struct KeyValue
{
    public string key;
    public string value;

    public KeyValue(string key, string value)
    {
        this.key = key;
        this.value = value;
    }
}

public static class WorldStateManager
{
    public static WorldState State;

    public static string saveFileName = "save.txt";
    public static string saveFilePath = Path.Combine(Application.persistentDataPath, "Save");
    public static void Init()
    {
        State = new WorldState();
        ReadFromFile();
    }


    /// <summary>
    /// ´æµµ
    /// </summary>
    public static void SaveToFile()
    {
        if (!Directory.Exists(saveFilePath))
            Directory.CreateDirectory(saveFilePath);

        string path = Path.Combine(saveFilePath, saveFileName);
        KVList data = new KVList();
        data.list = new List<KeyValue>();
        foreach (var item in State.DicState)
        {
            data.list.Add(new KeyValue(item.Key, item.Value));
        }

        string jsonStr = JsonUtility.ToJson(data);
        using (StreamWriter sw = new StreamWriter(path, append: false))
        {
            sw.Write(jsonStr);
        }
    }

    /// <summary>
    /// ¶Áµµ
    /// </summary>
    public static void ReadFromFile()
    {
        string path = Path.Combine(saveFilePath, saveFileName);
        using (StreamReader sr = new StreamReader(path))
        {
            string jsonStr = sr.ReadToEnd();
            KVList data = JsonUtility.FromJson<KVList>(jsonStr);
            foreach (var item in data.list)
            {
                State.SetValue(item.key, item.value);
            }
        }
    }

    public static void SetValues(List<WorldStateKV> list)
    {
        foreach (var item in list)
        {
            State.SetValue(item.key, item.value);
        }
        SaveToFile();
    }

    public static bool Check(List<WorldStateKV> list)
    {
        if (list == null || list.Count == 0)
            return true;

        foreach (var item in list)
        {
            bool result = false;
            switch (item.valueType)
            {
                case EValueType.Int:
                    result = CheckInt(item.key, int.Parse(item.value), item.op);
                    break;
                case EValueType.String:
                    result = CheckString(item.key, item.value, item.op);
                    break;
                case EValueType.Bool:
                    result = CheckBool(item.key, item.value.ToLower() == "true", item.op);
                    break;
                default:
                    break;
            }

            if (!result)
                return false;
        }
        return true;
    }

    public static bool CheckString(string key, string value, EOperation op)
    {
        string curValue = State.GetString(key);
        switch (op)
        {
            case EOperation.Equals:
                return curValue.Equals(value);
                break;
            case EOperation.NotEquals:
                return !curValue.Equals(value);
                break;
            default:
                break;
        }

        return false;
    }

    public static bool CheckBool(string key, bool value, EOperation op)
    {
        bool curValue = State.GetBool(key);
        switch (op)
        {
            case EOperation.Equals:
                return curValue == value;
                break;
            case EOperation.NotEquals:
                return curValue != value;
                break;
            default:
                break;
        }

        return false;
    }

    public static bool CheckInt(string key, int value, EOperation op)
    {
        int curValue = State.GetInt(key);
        switch (op)
        {
            case EOperation.Equals:
                return curValue == value;
                break;
            case EOperation.NotEquals:
                return curValue != value;
                break;
            case EOperation.Less:
                return curValue < value;
                break;
            case EOperation.Lagger:
                return curValue > value;
                break;
            default:
                break;
        }

        return false;
    }
}
