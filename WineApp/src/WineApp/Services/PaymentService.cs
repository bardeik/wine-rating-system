using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public Task<List<Payment>> GetAllPaymentsAsync() => _paymentRepository.GetAllPaymentsAsync();
    public Task<Payment?> GetPaymentByIdAsync(string id) => _paymentRepository.GetPaymentByIdAsync(id);
    public Task<List<Payment>> GetPaymentsByProducerIdAsync(string producerId) => _paymentRepository.GetPaymentsByProducerIdAsync(producerId);
    public Task<List<Payment>> GetPaymentsByEventIdAsync(string eventId) => _paymentRepository.GetPaymentsByEventIdAsync(eventId);
    public Task<List<Payment>> GetUnpaidPaymentsAsync() => _paymentRepository.GetUnpaidPaymentsAsync();
    public Task<List<Payment>> GetPaymentsWithoutReceiptAsync() => _paymentRepository.GetPaymentsWithoutReceiptAsync();
    public Task AddPaymentAsync(Payment payment) => _paymentRepository.AddPaymentAsync(payment);
    public Task UpdatePaymentAsync(Payment payment) => _paymentRepository.UpdatePaymentAsync(payment);
    public Task DeletePaymentAsync(string id) => _paymentRepository.DeletePaymentAsync(id);
}
