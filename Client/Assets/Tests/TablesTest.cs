using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using cfg;
using SimpleJSON;

public class TablesTest:MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PawnTable pawn = Game1ConfigManager._tables.PawnTable;
            Debug.Log(pawn.DataList[1].Name);
        }
    }
    
}
