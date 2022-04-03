namespace API.Entities.OrderAggregate; 

public enum OrderStatus {
    PaymentPending,
    PaymentReceived,
    PaymentFailed
}