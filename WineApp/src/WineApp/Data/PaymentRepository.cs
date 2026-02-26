using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class PaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<Payment> _payments;

    public PaymentRepository(WineMongoDbContext context) =>
        _payments = context.Payments;

    public List<Payment> GetAllPayments() => 
        _payments.Find(_ => true).ToList();

    public Payment? GetPaymentById(string id) => 
        _payments.Find(p => p.PaymentId == id).FirstOrDefault();

    public List<Payment> GetPaymentsByProducerId(string producerId) => 
        _payments.Find(p => p.WineProducerId == producerId).ToList();

    public List<Payment> GetPaymentsByEventId(string eventId) => 
        _payments.Find(p => p.EventId == eventId).ToList();

    public List<Payment> GetUnpaidPayments() => 
        _payments.Find(p => !p.IsPaid).ToList();

    public List<Payment> GetPaymentsWithoutReceipt() => 
        _payments.Find(p => p.IsPaid && !p.ReceiptSent).ToList();

    public void AddPayment(Payment payment) => 
        _payments.InsertOne(payment);

    public void UpdatePayment(Payment payment) => 
        _payments.ReplaceOne(p => p.PaymentId == payment.PaymentId, payment);

    public void DeletePayment(string id) => 
        _payments.DeleteOne(p => p.PaymentId == id);
}
