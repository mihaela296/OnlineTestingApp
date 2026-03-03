using System;
using System.Collections.Generic;

namespace OnlineTestingApp.Models
{
    public class UserDevice
    {
        public int DeviceId { get; set; }        // ID устройства (первичный ключ)
        public int UserId { get; set; }           // ID пользователя (внешний ключ)
        public string? DeviceName { get; set; }   // Название устройства
        public string? DeviceType { get; set; }   // Тип (Windows, Android, iOS)
        public string? DeviceToken { get; set; }  // Токен для push-уведомлений
        public DateTime LastActive { get; set; }  // Последняя активность
        public bool IsActive { get; set; } = true; // Активно ли устройство

        // Навигационные свойства
        public User? User { get; set; }           // Ссылка на пользователя
        public ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>(); // Попытки с этого устройства
    }
}