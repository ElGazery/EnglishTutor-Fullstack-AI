using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EnglishTutor.Application.Features.Accounts.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<RegisterResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse<RegisterResponseDto>> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                GradeLevel = request.GradeLevel
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "DuplicateEmail" || e.Code == "DuplicateUserName"))
                {
                    throw new BadRequestCustomException("A user with this email already exists.");
                }

                throw new BadRequestCustomException("Registration failed.", result.Errors.Select(e => e.Description));
            }

            var token = await _tokenService.GenerateTokenAsync(user);

            var responseDto = new RegisterResponseDto(
                user.Id,
                user.FullName ?? string.Empty,
                user.Email ?? string.Empty,
                user.GradeLevel,
                token);

            return new ApiResponse<RegisterResponseDto>(responseDto, "Registration successful.", 201);
        }
    }
}
