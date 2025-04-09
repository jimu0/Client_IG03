using UnityEngine;

/// <summary>
/// 描述基础单位状态的类，用于管理单位的核心属性和行为状态。
/// </summary>
[System.Serializable]
public class PawnState
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    //public Quaternion MeshRot { get; set; }
    public GameObject MountObj { get; set; }


    public PawnState(Vector3 position, Quaternion rotation, GameObject mountObj)
    {
        this.Position = position;
        this.Rotation = rotation;
        this.MountObj = mountObj;

    }

    public void SetTsf(Transform tsf)
    {
        Position = tsf.position;
        Rotation = tsf.rotation;
    }
    public void SetPos(Vector3 pos,Quaternion rot)
    {
        Position = pos;
        Rotation = rot;
    }
    public void SetPos(Vector3 pos,Quaternion rot, GameObject mountObj)
    {
        Position = pos;
        Rotation = rot;
        MountObj = mountObj;
    }
    
    /// <summary>
    /// 更新Pawn的位置。
    /// </summary>
    public void SetPosition(Vector3 pos) { Position = pos; }

    /// <summary>
    /// 更新旋转
    /// </summary>
    public void SetRotation(Quaternion rot) { Rotation = rot; }
    
    /// <summary>
    /// 更新位置和旋转
    /// </summary>
    public void SetPositionAndRotation(Vector3 pos, Quaternion rot) { Position = pos;Rotation = rot; }
    
    /// <summary>
    /// 更新挂载对象
    /// </summary>
    public void SetMountObj(GameObject obj) { MountObj = obj; }



    public override string ToString()
    {
        return $"位置: {Position}, 旋转: {Rotation}, 挂载对象: {MountObj.name}";
    }

    /// <summary>
    ///   <para>The red axis of the transform in world space.</para>
    /// </summary>
    public Vector3 right
    {
        get => this.Rotation * Vector3.right;
        set => this.Rotation = Quaternion.FromToRotation(Vector3.right, value);
    }

    /// <summary>
    ///   <para>The green axis of the transform in world space.</para>
    /// </summary>
    public Vector3 up
    {
        get => this.Rotation * Vector3.up;
        set => this.Rotation = Quaternion.FromToRotation(Vector3.up, value);
    }

    /// <summary>
    ///   <para>Returns a normalized vector representing the blue axis of the transform in world space.</para>
    /// </summary>
    public Vector3 forward
    {
        get => this.Rotation * Vector3.forward;
        set => this.Rotation = Quaternion.LookRotation(value);
    }
}