
namespace AorusLcdServiceLinux.Abstract
{
    internal interface ICpuApi
    {
        int GetCpuCoreCount();
        Task<string> GetCpuFrequencyAsync();
        string GetCpuManufacturer();
        string GetCpuName();
        double GetCpuPower();
        double GetCpuTemperature();
        int GetCpuThreadCount();
        Task<int> GetCpuUsageAsync();
        float GetMaxCpuFrequency();
        int GetVramTemperature();
    }
}
