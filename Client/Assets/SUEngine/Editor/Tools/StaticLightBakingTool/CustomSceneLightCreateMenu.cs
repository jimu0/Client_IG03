using Tools.StaticLightBakingTool.Baking;
using Tools.StaticLightBakingTool.Light;
using UnityEditor;
using UnityEngine;

namespace Tools.StaticLightBakingTool
{
    public static class CustomSceneLightCreateMenu
    {
        private static GameObject bakingLamp;
        
        [MenuItem("GameObject/Light/Baking Lamp/Static Sun Light", false, 10)]
        private static void CreateStaticSunLight(MenuCommand menuCommand)
        {
            CreateNewInstance<SunLightMonoBehaviour>("bakingLamp/staticSunlight", Vector3.zero, Vector3.forward, true);
            
        }

        [MenuItem("GameObject/Light/Baking Lamp/Static Ambient Light", false, 11)]
        private static void CreateStaticAmbientLight(MenuCommand menuCommand)
        {
            CreateNewInstance<AmbientLightMonoBehaviour>("bakingLamp/staticAmbientLight", Vector3.zero, Vector3.forward, true);
            
        }

        [MenuItem("GameObject/Light/Baking Lamp/Static Area Light", false, 12)]
        private static void CreateStaticAreaLight(MenuCommand menuCommand)
        {
            Vector3 r = (Vector3.zero - Vector3.up * 5).normalized;
            CreateNewInstance<AreaLightMonoBehaviour>("bakingLamp/staticAreaLight", Vector3.up * 5,r, true);
            
        }
        
        [MenuItem("GameObject/Light/Baking Lamp/Baking Box", false, 20)]
        private static void CreateStaticBakingBox(MenuCommand menuCommand)
        { 
            CreateNewInstance<BakingBoxMonoBehaviour>("bakingLamp/bakingBox", Vector3.zero, Vector3.forward, false);
            
        }
        
        /// <summary>
        /// 实例化光源
        /// </summary>
        /// <param name="name">命名</param>
        /// <param name="pos">位置</param>
        /// <param name="rot">旋转</param>
        /// <param name="gi">是否开启ContributeGI</param>
        /// <typeparam name="T">灯光类型</typeparam>
        private static void CreateNewInstance<T>(string name,Vector3 pos,Vector3 rot, bool gi)where T : Component
        {
            string rootName = null;
            string objName = null;
            if (name.Contains('/'))
            {
                int index = name.IndexOf('/');
                if (index >= 0)
                {
                    rootName = name[..index];
                    objName = name[(index + 1)..];
                }
            }
            else
            {
                objName = name;
            }

            GameObject obj = new();
            if (objName != null) obj.name = objName;
            obj.AddComponent<T>();
            if (gi) GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.ContributeGI);
            obj.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(rot));
            if(rootName != null)
            {
                BakingLampMonoBehaviour[] bl = Object.FindObjectsOfType<BakingLampMonoBehaviour>();
                if (bl.Length>0)
                {
                    bakingLamp = bl[0].gameObject;
                }
                else
                {
                    bakingLamp = new GameObject(rootName);
                    bakingLamp.AddComponent<BakingLampMonoBehaviour>();
                }
                obj.transform.parent = bakingLamp.transform;
            }
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeObject = obj;
        }
        
    }
}
    
