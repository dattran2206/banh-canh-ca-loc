using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Tables.Commands.AddTable
{
    public record AddTableCommand(int Number, int AreaId, int Capacity) : ICommand<Result<Table>>;

    public class AddTableCommandValidator : AbstractValidator<AddTableCommand>
    {
        public AddTableCommandValidator()
        {
            RuleFor(x => x.Number).GreaterThan(0).WithMessage("Số bàn phải lớn hơn 0");
            RuleFor(x => x.AreaId).GreaterThan(0).WithMessage("Khu vực không hợp lệ");
            RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Sức chứa phải lớn hơn 0");
        }
    }

    public class AddTableCommandHandler : ICommandHandler<AddTableCommand, Result<Table>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddTableCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Table>> Handle(AddTableCommand request, CancellationToken cancellationToken)
        {
            var existingTable = await _unitOfWork.Tables
                .FirstOrDefaultAsync(t => t.Number == request.Number, cancellationToken);

            if (existingTable != null)
            {
                return Result.Failure<Table>(new Error("Table.DuplicateNumber", $"Số bàn {request.Number} đã tồn tại"));
            }

            var table = new Table
            {
                Number = request.Number,
                AreaId = request.AreaId,
                Capacity = request.Capacity
            };

            await _unitOfWork.Tables.AddAsync(table, cancellationToken);

            return table;
        }
    }
}
