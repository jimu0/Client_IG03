using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class SetLayerTool: EditorWindow
{
    private int sortingLayerid = 0;
    private bool spriteRender = true;
    private bool meshRender = true;
    static string[] sortingLayerName;
    static int[] sortingLayerValue;
    static List<GameObject> prefabs;
    [MenuItem("Assets/Set Layer")]
    static void Init()
    {
        SortingLayer[] stl = SortingLayer.layers;
        sortingLayerName = new string[stl.Length];
        sortingLayerValue = new int[stl.Length];
        for (int i = 0; i < stl.Length; i++)
        {
            sortingLayerName[i] = stl[i].name;
        }

        for (int i = 0; i < sortingLayerName.Length; i++)
        {
            sortingLayerValue[i] = i;
        }
        SetLayerTool so = (SetLayerTool) EditorWindow.GetWindow(typeof(SetLayerTool), false, "Set sorting layer and order in layer", true);
        so.position = new Rect(600, 600,600,100);
        so.maxSize = so.minSize = new Vector2(600,100);
        so.Show();
    }
    [MenuItem("Assets/Set Layer", true)]
    static bool ValidateSelection()
    {
        string pathGUID = Selection.assetGUIDs[0];
        string _animationsTopPath = AssetDatabase.GUIDToAssetPath(pathGUID);
        if (_animationsTopPath != null && Directory.Exists(_animationsTopPath))
        {
            string[] prefabsPath = Directory.GetFiles(_animationsTopPath, "*.prefab",SearchOption.AllDirectories);
            prefabs = new List<GameObject>();
            for (int i = 0; i < prefabsPath.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath[i]);
                prefabs.Add(prefab);
            }
            if (prefabs.Count > 0)
            {
                return true;
            }
        }
        if (Selection.gameObjects != null)
        {
            prefabs = new List<GameObject>();
            foreach (var go in Selection.gameObjects)
            {
                string path = AssetDatabase.GetAssetPath(go);
                if (path.EndsWith(".prefab"))
                {
                    prefabs.Add(go);
                }
            }
            if (prefabs.Count > 0)
            {
                return true;
            }
        }
        return false;
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("");
        GUILayout.BeginHorizontal();
        spriteRender = EditorGUILayout.Toggle("Sprite Render", spriteRender);
        meshRender = EditorGUILayout.Toggle("Mesh Render", meshRender);
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        sortingLayerid = EditorGUILayout.IntPopup("Sorting layer",sortingLayerid,sortingLayerName,sortingLayerValue);
        EditorGUILayout.Space();
        if (GUILayout.Button("Set Layer"))
        {
            foreach (var so in prefabs)
            {
                if(spriteRender)
                {
                    SpriteRenderer[] srs = so.GetComponentsInChildren<SpriteRenderer>();
                    if (srs != null)
                    {
                        foreach (var sr in srs)
                        {
                            sr.sortingLayerName = sortingLayerName[sortingLayerid];
                        }
                    }
                }
                if(meshRender)
                {
                    MeshRenderer[] mrs = so.GetComponentsInChildren<MeshRenderer>();
                    if (mrs != null)
                    {
                        foreach (MeshRenderer mr in mrs)
                        {
                            mr.sortingLayerName = sortingLayerName[sortingLayerid];
                        }
                    }
                }
                PrefabUtility.SavePrefabAsset(so);
            }
            Close();
        }
        
    }

} 
