using WineApp.Models;

namespace WineApp.Services;

public interface IPaymentService
{
    Task<List<Payment>> GetAllPaymentsAsync();
    Task<Payment?> GetPaymentByIdAsync(string id);
    Task<List<Payment>> GetPaymentsByProducerIdAsync(string producerId);
    Task<List<Payment>> GetPaymentsByEventIdAsync(string eventId);
    Task<List<Payment>> GetUnpaidPaymentsAsync();
    Task<List<Payment>> GetPaymentsWithoutReceiptAsync();
    Task AddPaymentAsync(Payment payment);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(string id);
}
