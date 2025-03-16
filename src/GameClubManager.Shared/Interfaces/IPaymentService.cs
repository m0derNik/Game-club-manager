using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;

namespace GameClubManager.Shared.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetUserPaymentsAsync(int userId);
        Task<bool> ProcessPaymentAsync(int paymentId);
        Task<bool> RefundPaymentAsync(int paymentId);
        Task<decimal> GetUserBalanceAsync(int userId);
        Task<bool> TopUpBalanceAsync(int userId, decimal amount);
        Task<bool> DeductBalanceAsync(int userId, decimal amount);
        Task<IEnumerable<Payment>> GetPaymentHistoryAsync(int userId, DateTime from, DateTime to);
        Task<string> GeneratePaymentLinkAsync(Payment payment);
    }
} 