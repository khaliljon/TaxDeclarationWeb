using System;

namespace TaxDeclarationWeb.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        // Id пользователя (может быть null, если действие без авторизации)
        public string UserId { get; set; }

        // Email или имя пользователя (для удобства)
        public string UserEmail { get; set; }

        // Краткое название действия (например, "Удаление пользователя")
        public string Action { get; set; }

        // Время события (UTC)
        public DateTime Timestamp { get; set; }

        // Дополнительные детали (например, что именно удалено, параметры, изменения)
        public string Details { get; set; }
    }
}