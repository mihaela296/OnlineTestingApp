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

        // Получение информации об устройстве
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

        // Регистрация текущего устройства
        public async Task RegisterCurrentDeviceAsync(int userId)
        {
            try
            {
                var deviceId = await SecureStorage.GetAsync("device_id");
                
                // Если устройство уже зарегистрировано, обновляем дату активности
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

                // Регистрируем новое устройство
                var newDevice = new UserDevice
                {
                    UserId = userId,
                    DeviceName = DeviceInfo.Current.Name ?? "Unknown Device",
                    DeviceType = GetDeviceType(),
                    LastActive = DateTime.UtcNow,
                    IsActive = true,
                    DeviceToken = await SecureStorage.GetAsync("push_token") // Для push-уведомлений
                };

                _dbContext.Devices.Add(newDevice);
                await _dbContext.SaveChangesAsync();

                // Сохраняем ID устройства
                await SecureStorage.SetAsync("device_id", newDevice.DeviceId.ToString());
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем выполнение
                System.Diagnostics.Debug.WriteLine($"Ошибка регистрации устройства: {ex.Message}");
            }
        }

        // Отметить устройство как неактивное при выходе
        public async Task DeactivateCurrentDeviceAsync()
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
    }
}