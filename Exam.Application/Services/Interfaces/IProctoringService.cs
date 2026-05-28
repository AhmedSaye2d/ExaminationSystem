using Exam.Application.Dto.Proctoring;

namespace Exam.Application.Services.Interfaces
{
    public interface IProctoringService
    {
        Task<FastApiResponseDto> DetectCheatingAsync(ProctoringFrameRequest request, CancellationToken cancellationToken);
        Task<object> ProcessVideoAsync(ProctoringVideoRequest request, CancellationToken cancellationToken);
    }
}
