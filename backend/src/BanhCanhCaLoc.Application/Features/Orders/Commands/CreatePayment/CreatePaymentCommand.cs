using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Orders.Commands.CreatePayment
{
    public record CreatePaymentCommand(Guid OrderId, decimal TotalAmount, string PaymentMethod, Guid? UserId) : ICommand<Result<Payment>>;

    public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
    {
        public CreatePaymentCommandValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId không được rỗng");
            RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0).WithMessage("Số tiền thanh toán không hợp lệ");
            RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage("Phương thức thanh toán không được để trống");
        }
    }

    public class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand, Result<Payment>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreatePaymentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Payment>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result.Failure<Payment>(new Error("Order.NotFound", "Không tìm thấy đơn hàng"));
            }

            if (order.Status == "paid")
            {
                return Result.Failure<Payment>(new Error("Order.AlreadyPaid", "Hóa đơn này đã được thanh toán trước đó"));
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                TotalAmount = request.TotalAmount,
                PaidAt = DateTime.UtcNow,
                PaymentMethod = request.PaymentMethod
            };

            await _unitOfWork.Repository<Payment, Guid>().AddAsync(payment, cancellationToken);

            order.Status = "paid";
            _unitOfWork.Orders.Update(order);

            if (order.ShiftId.HasValue)
            {
                var shift = await _unitOfWork.Repository<Shift, Guid>().GetByIdAsync(order.ShiftId.Value, cancellationToken);
                if (shift != null)
                {
                    shift.TotalRevenue += request.TotalAmount;
                    shift.TotalBills += 1;
                    _unitOfWork.Repository<Shift, Guid>().Update(shift);
                }
            }

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "payment",
                Detail = $"Thanh toán order {order.OrderNumber} bàn {order.TableId}, {request.TotalAmount:N0}đ bằng {request.PaymentMethod}",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return payment;
        }
    }
}
