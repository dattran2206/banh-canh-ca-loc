using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Staff.Commands.AddStaff
{
    public record AddStaffCommand(string Username, string Password, string Role, string FullName, Guid? CurrentUserId) : ICommand<Result<User>>;

    public class AddStaffCommandValidator : AbstractValidator<AddStaffCommand>
    {
        public AddStaffCommandValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Tên tài khoản không được để trống")
                               .MaximumLength(50).WithMessage("Tên tài khoản tối đa 50 ký tự");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Mật khẩu không được để trống");
            RuleFor(x => x.Role).NotEmpty().WithMessage("Vai trò không được để trống")
                               .MaximumLength(20).WithMessage("Vai trò tối đa 20 ký tự");
            RuleFor(x => x.FullName).NotEmpty().WithMessage("Họ tên không được để trống")
                                   .MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự");
        }
    }

    public class AddStaffCommandHandler : ICommandHandler<AddStaffCommand, Result<User>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddStaffCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<User>> Handle(AddStaffCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
            if (existing != null)
            {
                return Result.Failure<User>(new Error("Staff.DuplicateUsername", "Tên tài khoản đã tồn tại"));
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                FullName = request.FullName,
                IsActive = true
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.CurrentUserId,
                Action = "add_user",
                Detail = $"Thêm nhân viên {request.FullName} ({request.Role})",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return user;
        }
    }
}
