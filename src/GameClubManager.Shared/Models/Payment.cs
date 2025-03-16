using System;

namespace GameClubManager.Shared.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentType Type { get; set; }
        public PaymentStatus Status { get; set; }
        public string TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string PaymentProvider { get; set; } // YooKassa, QIWI, etc.
    }

    public enum PaymentType
    {
        BookingPayment,
        BalanceTopUp,
        PenaltyPayment
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Refunded
    }
} 