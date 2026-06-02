using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Application.Common.Models;
using BanhCanhCaLoc.Application.Common.Interfaces.Services;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Application.Features.Auth.Common;

namespace BanhCanhCaLoc.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(string Username, string Password) : ICommand<Result<AuthResult>>;

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Tên đăng nhập không được để trống");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Mật khẩu không được để trống");
        }
    }

    public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<AuthResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public LoginCommandHandler(IUnitOfWork unitOfWork, IJwtTokenGenerator tokenGenerator)
        {
            _unitOfWork = unitOfWork;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<Result<AuthResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return Result.Failure<AuthResult>(new Error("Auth.InvalidCredentials", "Tài khoản không tồn tại hoặc đã bị khóa"));
            }

            bool match = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!match)
            {
                return Result.Failure<AuthResult>(new Error("Auth.InvalidCredentials", "Mật khẩu không đúng"));
            }

            var token = _tokenGenerator.GenerateToken(user);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Action = "login",
                Detail = "Đăng nhập",
                Timestamp = DateTime.UtcNow
            };
            
            await _unitOfWork.Repository<ActivityLog, Guid>().AddAsync(activityLog, cancellationToken);

            return new AuthResult(user, token);
        }
    }
}
