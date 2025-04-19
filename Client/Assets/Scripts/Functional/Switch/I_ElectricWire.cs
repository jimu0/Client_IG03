public interface I_ElectricWire
{
    bool IsActive { get; } // 当前状态
    int Kwh { get; set; } // 电量
    void ON();   // 开启
    void OFF(); // 关闭
    void Charge(int i); // 充电

}