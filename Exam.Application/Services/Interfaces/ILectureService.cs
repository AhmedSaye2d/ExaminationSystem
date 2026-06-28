using Exam.Application.Dto.Lecture;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Exam.Application.Services.Interfaces
{
    public interface ILectureService
    {
        Task<LectureDetailDTO> UploadLectureAsync(UploadLectureDTO dto, int instructorId, CancellationToken cancellationToken = default);
        Task<IEnumerable<LectureDTO>> GetCourseLecturesAsync(int courseId, int userId, CancellationToken cancellationToken = default);
        Task<LectureDetailDTO> GetLectureDetailsAsync(int lectureId, int userId, CancellationToken cancellationToken = default);
        Task<LectureDetailDTO> UpdateLectureAsync(int id, UpdateLectureDTO dto, int instructorId, CancellationToken cancellationToken = default);
        Task DeleteLectureAsync(int id, int instructorId, CancellationToken cancellationToken = default);
    }
}
