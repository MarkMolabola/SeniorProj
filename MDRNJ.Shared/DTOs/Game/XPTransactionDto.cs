using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Game
{
    public class XPTransactionDto
    {
        public Guid TransactionId { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
