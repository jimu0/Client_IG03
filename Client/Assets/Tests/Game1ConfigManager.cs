using System.IO;
using UnityEngine;
using cfg;
using SimpleJSON;

public class Game1ConfigManager : MonoBehaviour
{
    public static Tables _tables;

    public static void InitTables()
    {
        string gameConfDir = Path.Combine( "Assets/Scripts/GameConfig/Json");
        _tables = new Tables(file => JSON.Parse(File.ReadAllText($"{gameConfDir}/{file}.json")));
        Debug.Log("配置表加载完成");
    }
    public static PawnTable GetPawn()
    {
        if (_tables == null)
        {
            Debug.LogError("配置表未初始化！");
            return null;
        }
        return _tables.PawnTable;
    }

    private void Start()
    {
        InitTables();
        PawnTable pawn = GetPawn();
        foreach (PawnCfg data in pawn.DataList)
        {
            if (data != null) Debug.Log($"Pawn名称: {data.Name}，path: {data.Path}");
        }
    }
}
