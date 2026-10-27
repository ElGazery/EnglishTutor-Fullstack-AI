using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EnglishTutor.Application.Features.Accounts.Queries.GetProfile
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ApiResponse<GetProfileResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public GetProfileQueryHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<GetProfileResponseDto>> Handle(
            GetProfileQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedCustomException("User is not authenticated.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundCustomException("User profile not found.");
            }

            var dto = new GetProfileResponseDto(
                user.Id,
                user.FullName ?? string.Empty,
                user.Email ?? string.Empty,
                user.GradeLevel);

            return new ApiResponse<GetProfileResponseDto>(dto, "Profile retrieved successfully.", 200);
        }
    }
}
