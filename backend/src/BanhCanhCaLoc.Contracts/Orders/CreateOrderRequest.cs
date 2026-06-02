using System;
using System.Collections.Generic;

namespace BanhCanhCaLoc.Contracts.Orders
{
    public record CreateOrderRequest(int TableId, Guid? ShiftId, List<CreateOrderItemRequest> Items);
}
