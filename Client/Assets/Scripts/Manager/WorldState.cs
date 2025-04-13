using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldState
{
    public Dictionary<string, string> DicState;
    public WorldState()
    {
        DicState = new Dictionary<string, string>();
    }

    public void SetValue(string key, bool value)
    {
        if (DicState.ContainsKey(key))
            DicState[key] = value.ToString().ToLower();
        else
            DicState.Add(key, value.ToString().ToLower());
    }

    public void SetValue(string key, int value)
    {
        if (DicState.ContainsKey(key))
            DicState[key] = value.ToString();
        else
            DicState.Add(key, value.ToString());
    }

    public void SetValue(string key, string value)
    {
        if (DicState.ContainsKey(key))
            DicState[key] = value;
        else
            DicState.Add(key, value);
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (DicState.ContainsKey(key))
            return int.Parse(DicState[key]);

        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (DicState.ContainsKey(key))
            return DicState[key].Equals("true");
        return defaultValue;
    }

    public string GetString(string key, string defaultValue = "")
    {
        if (DicState.ContainsKey(key))
            return DicState[key];
        return defaultValue;
    }
}
