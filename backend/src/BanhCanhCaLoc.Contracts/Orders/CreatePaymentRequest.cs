using System;

namespace BanhCanhCaLoc.Contracts.Orders
{
    public record CreatePaymentRequest(Guid OrderId, decimal TotalAmount, string PaymentMethod);
}
