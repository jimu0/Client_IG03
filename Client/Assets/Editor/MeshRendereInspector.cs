using UnityEngine;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace SURender.Core
{
    [CustomEditor(typeof(MeshRenderer))]
    public class MeshRendereInspector : Editor
    {
        private MeshRenderer _meshRenderer;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _meshRenderer = target as MeshRenderer;
            if (!_meshRenderer) return;
            
            var layerNames = new string[SortingLayer.layers.Length]; //用于记录 layer 名的 string 数组
            for (var i = 0; i < SortingLayer.layers.Length; i++) layerNames[i] = SortingLayer.layers[i].name; //记录所有 layer 名到数组
            
            var layerValue = SortingLayer.GetLayerValueFromID(_meshRenderer.sortingLayerID) - SortingLayer.layers[0].value; //获取 meshRenderer 的 SortingLayer
            if (layerValue < 0 || layerValue >= layerNames.Length) layerValue = 0;
            layerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, layerNames); //画 meshRenderer 的枚举
            
            var layer = SortingLayer.layers[layerValue];
            _meshRenderer.sortingLayerName = layer.name;
            _meshRenderer.sortingLayerID = layer.id;
            _meshRenderer.sortingOrder = EditorGUILayout.IntField("Order in Layer", _meshRenderer.sortingOrder);
        }
    }
}