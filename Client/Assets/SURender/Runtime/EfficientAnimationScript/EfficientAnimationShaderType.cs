using System;

namespace StarUnion_EfficientAnimation
{
    [Flags]
    public enum EfficientAnimationShaderType
    {
        Base = 0,
        Gray = 1,
        //大于等于16的为时间触发效果，小于16的为各种状态效果
        Dissipation = 1 << 4,
        Flash = 1 << 5,
        Scale = 1 << 6,
        //混合多种效果
        DissipationAndFlash = Dissipation + Flash,
        DissipationAndScale = Dissipation + Scale,
        FlashAndScale = Flash + Scale,
        DissipationAndFlashAndScale = Dissipation + Flash + Scale
    }
}