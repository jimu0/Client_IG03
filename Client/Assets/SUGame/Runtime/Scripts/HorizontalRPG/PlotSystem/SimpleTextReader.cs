using System.Collections.Generic;
using UnityEngine;

public class SimpleTextReader : MonoBehaviour
{
    private TextAsset textAsset;
    public string dataPath = "TxtTest";
    private List<string> dataList = new();
    void Start()
    {
        textAsset = Resources.Load<TextAsset>(dataPath);
        if (textAsset != null)
        {
            string[] lines =
                textAsset.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length >= 2)
            {
                string secondLine = lines[1];
                string[] contentArray = secondLine.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                List<string> contentList = new(contentArray);
                for (int index = 0; index < contentList.Count; index++)
                {
                    string item = contentList[index];
                    Debug.Log($"{index}: {item}");
                }
            }




            // string fileContent = textAsset.text;
            // Debug.Log($"File Content:{fileContent}");
        }
        else
        {
            Debug.LogError($"指定路径:{dataPath}不存在，无法读取文件.");
        }
    }
}
