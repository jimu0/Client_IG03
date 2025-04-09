// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class MapManager : MonoBehaviour
// {
//     private QuadTree quadTree;
//     public GameObject player;
//     private float mapWidth = 20f;
//     private float mapHeight = 20f;
//     // 存储上一次视野范围内的区块
//     private List<GameObject> activeBlocks = new List<GameObject>();
//     void Start()
//     {
//         // 初始化四叉树
//         quadTree = new QuadTree(5, new Rect(0, 0, mapWidth, mapHeight));
//         
//         // 插入一些区块对象
//         for (int i = 0; i < 50; i++)
//         {
//             //GameObject block = new GameObject("Block" + i);
//             GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
//             block.name = "Block" + i;
//             block.transform.position = new Vector3(Random.Range(0, mapWidth), Random.Range(0, mapHeight), 0);
//             Rect blockRect = new Rect(block.transform.position.x, block.transform.position.y, 1, 1);
//             block.SetActive(false);
//             quadTree.Insert(block, blockRect);
//         }
//     }
//
//     void Update()
//     {
//         // 定义玩家视野范围
//         Rect viewRect = new Rect(player.transform.position.x - 12, player.transform.position.y - 7, 24, 14);
//         rect = viewRect;
//         // 获取当前视野内的所有区块
//         List<GameObject> visibleBlocks = new List<GameObject>();
//         quadTree.Retrieve(visibleBlocks, viewRect);
//
//         // 1. 隐藏之前激活的区块
//         foreach (GameObject block in activeBlocks)
//         {
//             block.SetActive(false);
//         }
//
//         // 2. 显示当前视野内的区块
//         foreach (GameObject block in visibleBlocks)
//         {
//             block.SetActive(true);
//         }
//
//         // 3. 更新activeBlocks列表，保存这次视野内的区块
//         activeBlocks = visibleBlocks;
//     }
//     
//     
//     public Rect rect; // 要绘制的Rect
//     public Color gizmoColor = Color.red; // Gizmo的颜色
//
//     private void OnDrawGizmos()
//     {
//         
//         // 设置Gizmo颜色
//         Gizmos.color = gizmoColor;
//
//         // 计算矩形的中心点和尺寸
//         Vector3 center = new Vector3(rect.x + rect.width / 2, rect.y + rect.height / 2, 0);
//         Vector3 size = new Vector3(rect.width, rect.height, 1);
//
//         // 绘制线框矩形（在场景视图中可见）
//         Gizmos.DrawWireCube(center, size);
//     }
//     
// }
//
