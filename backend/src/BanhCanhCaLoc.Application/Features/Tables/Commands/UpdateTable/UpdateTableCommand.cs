using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Tables.Commands.UpdateTable
{
    public record UpdateTableCommand(int Id, int Number, int AreaId, int Capacity) : ICommand<Result<Table>>;

    public class UpdateTableCommandValidator : AbstractValidator<UpdateTableCommand>
    {
        public UpdateTableCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id bàn không hợp lệ");
            RuleFor(x => x.Number).GreaterThan(0).WithMessage("Số bàn phải lớn hơn 0");
            RuleFor(x => x.AreaId).GreaterThan(0).WithMessage("Khu vực không hợp lệ");
            RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Sức chứa phải lớn hơn 0");
        }
    }

    public class UpdateTableCommandHandler : ICommandHandler<UpdateTableCommand, Result<Table>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTableCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Table>> Handle(UpdateTableCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Tables.GetByIdAsync(request.Id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure<Table>(new Error("Table.NotFound", "Không tìm thấy bàn ăn"));
            }

            var duplicate = await _unitOfWork.Tables
                .FirstOrDefaultAsync(t => t.Number == request.Number && t.Id != request.Id, cancellationToken);

            if (duplicate != null)
            {
                return Result.Failure<Table>(new Error("Table.DuplicateNumber", $"Số bàn {request.Number} đã tồn tại"));
            }

            existing.Number = request.Number;
            existing.AreaId = request.AreaId;
            existing.Capacity = request.Capacity;

            _unitOfWork.Tables.Update(existing);

            return existing;
        }
    }
}
