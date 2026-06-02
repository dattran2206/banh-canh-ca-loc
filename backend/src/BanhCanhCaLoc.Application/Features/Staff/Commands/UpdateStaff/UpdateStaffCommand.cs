using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Staff.Commands.UpdateStaff
{
    public record UpdateStaffCommand(Guid Id, string Username, string? Password, string Role, string FullName, Guid? CurrentUserId) : ICommand<Result<User>>;

    public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
    {
        public UpdateStaffCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id không được rỗng");
            RuleFor(x => x.Username).NotEmpty().WithMessage("Tên tài khoản không được để trống")
                               .MaximumLength(50).WithMessage("Tên tài khoản tối đa 50 ký tự");
            RuleFor(x => x.Role).NotEmpty().WithMessage("Vai trò không được để trống")
                               .MaximumLength(20).WithMessage("Vai trò tối đa 20 ký tự");
            RuleFor(x => x.FullName).NotEmpty().WithMessage("Họ tên không được để trống")
                                   .MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự");
        }
    }

    public class UpdateStaffCommandHandler : ICommandHandler<UpdateStaffCommand, Result<User>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStaffCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<User>> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
            {
                return Result.Failure<User>(new Error("Staff.NotFound", "Không tìm thấy tài khoản nhân viên"));
            }

            if (user.Username != request.Username)
            {
                var existing = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
                if (existing != null)
                {
                    return Result.Failure<User>(new Error("Staff.DuplicateUsername", "Tên tài khoản đã tồn tại"));
                }
            }

            user.Username = request.Username;
            user.FullName = request.FullName;
            user.Role = request.Role;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            _unitOfWork.Users.Update(user);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.CurrentUserId,
                Action = "edit_user",
                Detail = $"Sửa nhân viên {request.FullName} ({request.Role})",
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return user;
        }
    }
}
