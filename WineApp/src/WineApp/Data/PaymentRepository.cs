using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class PaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<Payment> _payments;

    public PaymentRepository(WineMongoDbContext context) =>
        _payments = context.Payments;

    public async Task<List<Payment>> GetAllPaymentsAsync() =>
        await _payments.Find(_ => true).ToListAsync();

    public async Task<Payment?> GetPaymentByIdAsync(string id) =>
        await _payments.Find(p => p.PaymentId == id).FirstOrDefaultAsync();

    public async Task<List<Payment>> GetPaymentsByProducerIdAsync(string producerId) =>
        await _payments.Find(p => p.WineProducerId == producerId).ToListAsync();

    public async Task<List<Payment>> GetPaymentsByEventIdAsync(string eventId) =>
        await _payments.Find(p => p.EventId == eventId).ToListAsync();

    public async Task<List<Payment>> GetUnpaidPaymentsAsync() =>
        await _payments.Find(p => !p.IsPaid).ToListAsync();

    public async Task<List<Payment>> GetPaymentsWithoutReceiptAsync() =>
        await _payments.Find(p => p.IsPaid && !p.ReceiptSent).ToListAsync();

    public async Task AddPaymentAsync(Payment payment) =>
        await _payments.InsertOneAsync(payment);

    public async Task UpdatePaymentAsync(Payment payment) =>
        await _payments.ReplaceOneAsync(p => p.PaymentId == payment.PaymentId, payment);

    public async Task DeletePaymentAsync(string id) =>
        await _payments.DeleteOneAsync(p => p.PaymentId == id);
}
