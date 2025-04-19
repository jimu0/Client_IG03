using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPreserver2 : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(FixSceneMaterials());
    }

    IEnumerator FixSceneMaterials()
    {
        yield return new WaitForEndOfFrame(); // 关键延迟！
    
        foreach (var renderer in FindObjectsOfType<Renderer>(true))
        {
            var mats = renderer.sharedMaterials;
            bool needUpdate = false;
        
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
            
                // 检测"粉紫"错误材质
                if (mats[i].shader.name.Contains("Error") || 
                    mats[i].shader.name == "Hidden/InternalErrorShader")
                {
                    // 尝试恢复原始Shader
                    var originalShader = Shader.Find(mats[i].shader.name.Replace("Error", ""));
                    if (originalShader != null)
                    {
                        mats[i].shader = originalShader;
                        needUpdate = true;
                    }
                    else
                    {
                        // 回退到URP默认Shader
                        mats[i].shader = Shader.Find("Universal Render Pipeline/Lit");
                        needUpdate = true;
                    }
                }
            }
        
            if (needUpdate) 
                renderer.sharedMaterials = mats;
        }
    }
}
