// using UnityEngine;
// using System.Collections.Generic;
//
// public class t1_QuadTree
// {
//     private int maxObjects = 4;// 每个节点允许的最大对象数
//     private int maxLevels = 5;// 四叉树最大层级
//     private int level;// 当前层级
//     private List<GameObject> objects;// 存储在这个节点中的对象
//     private Rect bounds;// 当前节点的区域范围
//     private QuadTree[] nodes;// 四个子节点
//
//     public QuadTree(int level, Rect bounds)
//     {
//         this.level = level;
//         this.bounds = bounds;
//         objects = new List<GameObject>();
//         nodes = new QuadTree[4];// 四个子节点
//     }
//
//     // 清除节点中的所有对象
//     public void Clear()
//     {
//         objects.Clear();
//         for (int i = 0; i < nodes.Length; i++)
//         {
//             if (nodes[i] != null)
//             {
//                 nodes[i].Clear();
//                 nodes[i] = null;
//             }
//         }
//     }
//
//     // 分裂节点，将其分为四个子节点
//     private void Split()
//     {
//         float subWidth = bounds.width / 2f;
//         float subHeight = bounds.height / 2f;
//         float x = bounds.x;
//         float y = bounds.y;
//
//         // 创建四个子区域
//         nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight));// 右上
//         nodes[1] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight));// 左上
//         nodes[2] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight));// 左下
//         nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight));// 右下
//     }
//
//     // 获取对象所在的象限 (0-3)
//     private int GetIndex(Rect rect)
//     {
//         int index = -1;
//         float verticalMidpoint = bounds.x + bounds.width / 2f;
//         float horizontalMidpoint = bounds.y + bounds.height / 2f;
//
//         bool topQuadrant = (rect.y < horizontalMidpoint && rect.y + rect.height < horizontalMidpoint);
//         bool bottomQuadrant = (rect.y > horizontalMidpoint);
//
//         if (rect.x < verticalMidpoint && rect.x + rect.width < verticalMidpoint)
//         {
//             if (topQuadrant)
//             {
//                 index = 1;// 左上
//             }
//             else if (bottomQuadrant)
//             {
//                 index = 2;// 左下
//             }
//         }
//         else if (rect.x > verticalMidpoint)
//         {
//             if (topQuadrant)
//             {
//                 index = 0;// 右上
//             }
//             else if (bottomQuadrant)
//             {
//                 index = 3;// 右下
//             }
//         }
//
//         return index;
//     }
//
//     // 将对象插入四叉树中
//     public void Insert(GameObject obj, Rect rect)
//     {
//         if (nodes[0] != null)
//         {
//             int index = GetIndex(rect);
//             if (index != -1)
//             {
//                 nodes[index].Insert(obj, rect);
//                 return;
//             }
//         }
//
//         objects.Add(obj);
//
//         if (objects.Count > maxObjects && level < maxLevels)
//         {
//             if (nodes[0] == null)
//             {
//                 Split();
//             }
//
//             int i = 0;
//             while (i < objects.Count)
//             {
//                 Rect objRect = new Rect(objects[i].transform.position.x, objects[i].transform.position.y, 1, 1); // 假设对象大小为1x1
//                 int index = GetIndex(objRect);
//                 if (index != -1)
//                 {
//                     GameObject removedObject = objects[i];
//                     objects.RemoveAt(i);
//                     nodes[index].Insert(removedObject, objRect);
//                 }
//                 else
//                 {
//                     i++;
//                 }
//             }
//         }
//     }
//
//     // 返回给定区域内的所有对象
//     public List<GameObject> Retrieve(List<GameObject> returnObjects, Rect rect)
//     {
//         int index = GetIndex(rect);
//         if (index != -1 && nodes[0] != null)
//         {
//             nodes[index].Retrieve(returnObjects, rect);
//         }
//
//         returnObjects.AddRange(objects);
//
//         return returnObjects;
//     }
// }
