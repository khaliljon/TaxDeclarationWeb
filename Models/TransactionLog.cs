using System;

namespace TaxDeclarationWeb.Models
{
    public class TransactionLog
    {
        public int Id { get; set; }

        // Id пользователя (из Identity), совершившего операцию
        public string UserId { get; set; }

        // Email/логин пользователя
        public string UserEmail { get; set; }

        // Тип операции: Insert, Update, Delete
        public string Operation { get; set; }

        // Имя сущности: Declaration, Taxpayer и др.
        public string Entity { get; set; }

        // Id сущности (например, ИИН, Id декларации и т.п.)
        public string EntityId { get; set; }

        // Время операции (UTC)
        public DateTime Timestamp { get; set; }

        // Детали — любые подробности (что изменено, значения, старые/новые данные и т.п.)
        public string Details { get; set; }
    }
}