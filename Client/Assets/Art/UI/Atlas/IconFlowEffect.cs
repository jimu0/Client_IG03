using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IconFlowEffect : MonoBehaviour
{
    public Vector4 uvOffsetScale; // UV偏移和缩放 (offsetX, offsetY, scaleX, scaleY)

    private MaterialPropertyBlock propertyBlock;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();

        // 设置UV参数
        SetUVParameters();
    }

    void SetUVParameters()
    {
        if (spriteRenderer != null)
        {
            // 获取当前MaterialPropertyBlock
            spriteRenderer.GetPropertyBlock(propertyBlock);

            // 设置UV偏移和缩放
            propertyBlock.SetVector("_IconOffsetScale", uvOffsetScale);

            // 应用MaterialPropertyBlock
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}