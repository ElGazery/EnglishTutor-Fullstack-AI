using EnglishTutor.Application.Common.ResponseBase.Response;
using MediatR;

namespace EnglishTutor.Application.Features.Accounts.Queries.GetProfile
{
    public record GetProfileQuery : IRequest<ApiResponse<GetProfileResponseDto>>;

    public record GetProfileResponseDto(
        string Id,
        string FullName,
        string Email,
        string? GradeLevel);
}
