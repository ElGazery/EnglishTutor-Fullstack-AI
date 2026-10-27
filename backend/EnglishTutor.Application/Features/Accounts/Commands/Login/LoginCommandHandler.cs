using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EnglishTutor.Application.Features.Accounts.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse<LoginResponseDto>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedCustomException("Invalid email or password.");
            }

            var token = await _tokenService.GenerateTokenAsync(user);

            var responseDto = new LoginResponseDto(
                user.Id,
                user.FullName ?? string.Empty,
                user.Email ?? string.Empty,
                user.GradeLevel,
                token);

            return new ApiResponse<LoginResponseDto>(responseDto, "Login successful.", 200);
        }
    }
}
