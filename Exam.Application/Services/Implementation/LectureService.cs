using AutoMapper;
using Exam.Application.Dto.Lecture;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces;
using Exam.Domain.Entities;
using Exam.Domain.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Exam.Application.Services.Implementation
{
    public class LectureService : ILectureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly long _maxVideoSizeLimit;
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".mov" };
        private readonly string[] _allowedVideoContentTypes = { "video/mp4", "video/webm", "video/quicktime" };

        public LectureService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IFileStorageService fileStorageService,
            ICurrentUserService currentUserService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;

            // Default to 2GB if not specified in config
            var sizeMB = configuration.GetValue<long>("FileSettings:MaxVideoSizeMB", 2048);
            _maxVideoSizeLimit = sizeMB * 1024 * 1024;
        }

        public async Task<LectureDetailDTO> UploadLectureAsync(UploadLectureDTO dto, int instructorId, CancellationToken cancellationToken = default)
        {
            // 1. Validate Course exists
            var course = await _unitOfWork.Repository<Course>().GetByIdAsync(dto.CourseId);
            if (course == null || course.IsDeleted)
                throw new ItemNotFoundException("Course not found.");

            // 2. Validate Ownership/Permission (Admin or assigned Instructor)
            var isAdmin = _currentUserService.IsAdmin();
            var isAssigned = await _unitOfWork.Repository<CourseInstructor>()
                .ExistsAsync(ci => ci.CourseId == dto.CourseId && ci.InstructorId == instructorId && !ci.IsDeleted);

            if (!isAdmin && !isAssigned)
                throw new UnauthorizedAccessException("You are not authorized to upload lectures for this course.");

            // 3. Validate Video File
            ValidateVideoFile(dto.VideoFile);

            // 4. Upload Video File
            string videoUrl;
            using (var stream = dto.VideoFile.OpenReadStream())
            {
                videoUrl = await _fileStorageService.UploadAsync(stream, dto.VideoFile.FileName, "uploads/lectures", cancellationToken);
            }

            // Create Lecture Entity
            var lecture = new Lecture
            {
                Title = dto.Title,
                Description = dto.Description,
                CourseId = dto.CourseId,
                InstructorId = instructorId,
                VideoUrl = videoUrl,
                FileSize = dto.VideoFile.Length,
                DurationSeconds = 0, // In production, we'd extract duration using a media parser or UI metadata
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Lecture>().AddAsync(lecture);
            await _unitOfWork.CompleteAsync();

            // 5. Upload Attachment Files (Optional)
            if (dto.AttachmentFiles != null && dto.AttachmentFiles.Any())
            {
                foreach (var file in dto.AttachmentFiles)
                {
                    string fileUrl;
                    using (var stream = file.OpenReadStream())
                    {
                        fileUrl = await _fileStorageService.UploadAsync(stream, file.FileName, "uploads/attachments", cancellationToken);
                    }

                    var attachment = new LectureAttachment
                    {
                        LectureId = lecture.Id,
                        FileName = file.FileName,
                        FileUrl = fileUrl,
                        FileSize = file.Length,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<LectureAttachment>().AddAsync(attachment);
                }
                await _unitOfWork.CompleteAsync();
            }

            // Refresh entity to load attachments
            var createdLecture = (await _unitOfWork.Repository<Lecture>()
                .FindAsync(l => l.Id == lecture.Id, "Attachments")).FirstOrDefault();

            var result = _mapper.Map<LectureDetailDTO>(createdLecture);
            result.VideoUrl = _fileStorageService.GetUrl(result.VideoUrl);
            foreach (var att in result.Attachments)
            {
                att.FileUrl = _fileStorageService.GetUrl(att.FileUrl);
            }

            return result;
        }

        public async Task<IEnumerable<LectureDTO>> GetCourseLecturesAsync(int courseId, int userId, CancellationToken cancellationToken = default)
        {
            // 1. Validate course exists
            var course = await _unitOfWork.Repository<Course>().GetByIdAsync(courseId);
            if (course == null || course.IsDeleted)
                throw new ItemNotFoundException("Course not found.");

            // 2. Validate permission
            var isAdmin = _currentUserService.IsAdmin();
            var isInstructorAssigned = await _unitOfWork.Repository<CourseInstructor>()
                .ExistsAsync(ci => ci.CourseId == courseId && ci.InstructorId == userId && !ci.IsDeleted);
            var isStudentEnrolled = await _unitOfWork.Repository<CourseStudent>()
                .ExistsAsync(cs => cs.CourseId == courseId && cs.StudentId == userId && !cs.IsDeleted);

            if (!isAdmin && !isInstructorAssigned && !isStudentEnrolled)
                throw new UnauthorizedAccessException("You are not authorized to view lectures for this course.");

            var lectures = await _unitOfWork.Repository<Lecture>()
                .FindAsync(l => l.CourseId == courseId);

            var result = _mapper.Map<IEnumerable<LectureDTO>>(lectures).ToList();
            foreach (var r in result)
            {
                r.VideoUrl = _fileStorageService.GetUrl(r.VideoUrl);
            }

            return result;
        }

        public async Task<LectureDetailDTO> GetLectureDetailsAsync(int lectureId, int userId, CancellationToken cancellationToken = default)
        {
            var lecture = (await _unitOfWork.Repository<Lecture>()
                .FindAsync(l => l.Id == lectureId, "Attachments")).FirstOrDefault();

            if (lecture == null)
                throw new ItemNotFoundException("Lecture not found.");

            // Validate permission on Course
            var isAdmin = _currentUserService.IsAdmin();
            var isInstructorAssigned = await _unitOfWork.Repository<CourseInstructor>()
                .ExistsAsync(ci => ci.CourseId == lecture.CourseId && ci.InstructorId == userId && !ci.IsDeleted);
            var isStudentEnrolled = await _unitOfWork.Repository<CourseStudent>()
                .ExistsAsync(cs => cs.CourseId == lecture.CourseId && cs.StudentId == userId && !cs.IsDeleted);

            if (!isAdmin && !isInstructorAssigned && !isStudentEnrolled)
                throw new UnauthorizedAccessException("You are not authorized to view this lecture.");

            var result = _mapper.Map<LectureDetailDTO>(lecture);
            result.VideoUrl = _fileStorageService.GetUrl(result.VideoUrl);
            foreach (var att in result.Attachments)
            {
                att.FileUrl = _fileStorageService.GetUrl(att.FileUrl);
            }

            return result;
        }

        public async Task<LectureDetailDTO> UpdateLectureAsync(int id, UpdateLectureDTO dto, int instructorId, CancellationToken cancellationToken = default)
        {
            var lecture = (await _unitOfWork.Repository<Lecture>()
                .FindAsync(l => l.Id == id, "Attachments")).FirstOrDefault();

            if (lecture == null)
                throw new ItemNotFoundException("Lecture not found.");

            // Validate Permission (Admin or assigned Instructor of the course)
            var isAdmin = _currentUserService.IsAdmin();
            var isAssigned = await _unitOfWork.Repository<CourseInstructor>()
                .ExistsAsync(ci => ci.CourseId == lecture.CourseId && ci.InstructorId == instructorId && !ci.IsDeleted);

            if (!isAdmin && !isAssigned)
                throw new UnauthorizedAccessException("You are not authorized to update this lecture.");

            lecture.Title = dto.Title;
            lecture.Description = dto.Description;
            lecture.UpdatedAt = DateTime.UtcNow;

            // Replace Video if provided
            if (dto.VideoFile != null)
            {
                ValidateVideoFile(dto.VideoFile);

                // Delete old video file
                await _fileStorageService.DeleteAsync(lecture.VideoUrl);

                // Upload new video file
                string newVideoUrl;
                using (var stream = dto.VideoFile.OpenReadStream())
                {
                    newVideoUrl = await _fileStorageService.UploadAsync(stream, dto.VideoFile.FileName, "uploads/lectures", cancellationToken);
                }
                lecture.VideoUrl = newVideoUrl;
                lecture.FileSize = dto.VideoFile.Length;
            }

            // Replace attachments: delete old ones first, then add new ones
            if (dto.AttachmentFiles != null && dto.AttachmentFiles.Any())
            {
                // Delete old attachment files from storage and soft-delete DB records
                foreach (var oldAttachment in lecture.Attachments)
                {
                    await _fileStorageService.DeleteAsync(oldAttachment.FileUrl);
                    await _unitOfWork.Repository<LectureAttachment>().DeleteAsync(oldAttachment.Id);
                }

                // Upload and save new attachments
                foreach (var file in dto.AttachmentFiles)
                {
                    string fileUrl;
                    using (var stream = file.OpenReadStream())
                    {
                        fileUrl = await _fileStorageService.UploadAsync(stream, file.FileName, "uploads/attachments", cancellationToken);
                    }

                    var attachment = new LectureAttachment
                    {
                        LectureId = lecture.Id,
                        FileName = file.FileName,
                        FileUrl = fileUrl,
                        FileSize = file.Length,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<LectureAttachment>().AddAsync(attachment);
                }
            }

            await _unitOfWork.Repository<Lecture>().UpdateAsync(lecture);
            await _unitOfWork.CompleteAsync(); // Persists: lecture update + deleted attachments + new attachments

            // Reload lecture to include attachments
            var updatedLecture = (await _unitOfWork.Repository<Lecture>()
                .FindAsync(l => l.Id == id && !l.IsDeleted, "Attachments")).FirstOrDefault();

            var result = _mapper.Map<LectureDetailDTO>(updatedLecture);
            result.VideoUrl = _fileStorageService.GetUrl(result.VideoUrl);
            foreach (var att in result.Attachments)
            {
                att.FileUrl = _fileStorageService.GetUrl(att.FileUrl);
            }

            return result;
        }

        public async Task DeleteLectureAsync(int id, int instructorId, CancellationToken cancellationToken = default)
        {
            var lecture = (await _unitOfWork.Repository<Lecture>()
                .FindAsync(l => l.Id == id, "Attachments")).FirstOrDefault();

            if (lecture == null)
                throw new ItemNotFoundException("Lecture not found.");

            // Validate Permission (Admin or assigned Instructor of the course)
            var isAdmin = _currentUserService.IsAdmin();
            var isAssigned = await _unitOfWork.Repository<CourseInstructor>()
                .ExistsAsync(ci => ci.CourseId == lecture.CourseId && ci.InstructorId == instructorId && !ci.IsDeleted);

            if (!isAdmin && !isAssigned)
                throw new UnauthorizedAccessException("You are not authorized to delete this lecture.");

            // Soft delete
            lecture.IsDeleted = true;
            lecture.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Lecture>().UpdateAsync(lecture);

            // Optional: delete physical files on soft delete, or keep them.
            // Under normal soft-delete architectures we leave physical files alone or schedule a cleanup task.
            // For now we just update DB record.
            
            await _unitOfWork.CompleteAsync();
        }

        private void ValidateVideoFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Video file is empty.");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedVideoExtensions.Contains(extension))
                throw new ArgumentException($"Invalid video format. Allowed formats: {string.Join(", ", _allowedVideoExtensions)}");

            // Validate MIME / ContentType to prevent disguised file uploads
            var contentType = file.ContentType?.ToLower() ?? string.Empty;
            if (!_allowedVideoContentTypes.Contains(contentType))
                throw new ArgumentException($"Invalid video content type '{file.ContentType}'. Allowed types: mp4, webm, mov.");

            // Validate file size
            if (file.Length > _maxVideoSizeLimit)
                throw new ArgumentException($"Video file size exceeds the allowed limit of {_maxVideoSizeLimit / (1024 * 1024)} MB.");
        }
    }
}
