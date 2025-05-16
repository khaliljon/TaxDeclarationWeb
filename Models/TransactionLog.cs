using System;

namespace TaxDeclarationWeb.Models
{
    public class TransactionLog
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string UserEmail { get; set; }

        public string Operation { get; set; }

        public string Entity { get; set; }

        public string EntityId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Details { get; set; }
    }
}