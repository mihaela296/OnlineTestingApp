using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Models;

namespace OnlineTestingApp.Services
{
    public class DeviceService
    {
        private readonly AppDbContext _dbContext;

        public DeviceService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private string GetDeviceType()
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
                return "Android";
            if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
                return "iOS";
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
                return "Windows";
            if (DeviceInfo.Current.Platform == DevicePlatform.macOS)
                return "macOS";
            return "Unknown";
        }

        public async Task RegisterCurrentDeviceAsync(int userId)
        {
            try
            {
                var deviceId = await SecureStorage.GetAsync("device_id");
                
                if (!string.IsNullOrEmpty(deviceId) && int.TryParse(deviceId, out int existingDeviceId))
                {
                    var device = await _dbContext.Devices.FindAsync(existingDeviceId);
                    if (device != null)
                    {
                        device.LastActive = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                        return;
                    }
                }

                var newDevice = new UserDevice
                {
                    UserId = userId,
                    DeviceName = DeviceInfo.Current.Name ?? "Unknown Device",
                    DeviceType = GetDeviceType(),
                    LastActive = DateTime.UtcNow,
                    IsActive = true,
                };

                _dbContext.Devices.Add(newDevice);
                await _dbContext.SaveChangesAsync();

                await SecureStorage.SetAsync("device_id", newDevice.DeviceId.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка регистрации устройства: {ex.Message}");
            }
        }

        public async Task DeactivateCurrentDeviceAsync()
        {
            try
            {
                var deviceId = await SecureStorage.GetAsync("device_id");
                if (!string.IsNullOrEmpty(deviceId) && int.TryParse(deviceId, out int existingDeviceId))
                {
                    var device = await _dbContext.Devices.FindAsync(existingDeviceId);
                    if (device != null)
                    {
                        device.IsActive = false;
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка деактивации устройства: {ex.Message}");
            }
        }
    }
}
