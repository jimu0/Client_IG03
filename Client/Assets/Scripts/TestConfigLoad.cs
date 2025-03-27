using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestConfigLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ConfigManager.Init();
        Debug.Log(ConfigManager.GetPropCfg(10000));
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
