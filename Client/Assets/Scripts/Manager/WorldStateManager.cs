using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EOperation
{
    Equals,
    NotEquals,
    Less,
    Lagger,
}

public static class WorldStateManager
{
    public static WorldState State;

    public static void Init()
    {
        // todo ¶Áµµ
        State = new WorldState();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SetValue(string key, string value)
    {
        State.SetValue(key, value);
    }

    public static void SetValues(List<WorldStateKV> list)
    {
        foreach (var item in list)
        {
            State.SetValue(item.key, item.value);
        }
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
