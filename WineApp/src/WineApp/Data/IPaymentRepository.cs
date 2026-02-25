using WineApp.Models;

namespace WineApp.Data;

public interface IPaymentRepository
{
    List<Payment> GetAllPayments();
    Payment? GetPaymentById(string id);
    List<Payment> GetPaymentsByProducerId(string producerId);
    List<Payment> GetPaymentsByEventId(string eventId);
    List<Payment> GetUnpaidPayments();
    List<Payment> GetPaymentsWithoutReceipt();
    void AddPayment(Payment payment);
    void UpdatePayment(Payment payment);
    void DeletePayment(string id);
}
