using System.Text.Json.Serialization;

namespace Exam.Application.Dto.Proctoring
{
    public class EyeTrackingDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("timer_seconds")]
        public double TimerSeconds { get; set; }

        [JsonPropertyName("threshold_seconds")]
        public double ThresholdSeconds { get; set; }

        [JsonPropertyName("violation")]
        public bool Violation { get; set; }
    }

    public class HeadPoseDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("violation")]
        public bool Violation { get; set; }
    }

    public class PhoneDetectionDto
    {
        [JsonPropertyName("detected")]
        public bool Detected { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("timer_seconds")]
        public double TimerSeconds { get; set; }

        [JsonPropertyName("violation")]
        public bool Violation { get; set; }
    }

    public class PersonDetectionDto
    {
        [JsonPropertyName("person_count")]
        public int PersonCount { get; set; }

        [JsonPropertyName("timer_seconds")]
        public double TimerSeconds { get; set; }

        [JsonPropertyName("violation")]
        public bool Violation { get; set; }
    }

    public class FaceDetectionDto
    {
        [JsonPropertyName("face_present")]
        public bool FacePresent { get; set; }

        [JsonPropertyName("timer_seconds")]
        public double TimerSeconds { get; set; }

        [JsonPropertyName("threshold_seconds")]
        public double ThresholdSeconds { get; set; }

        [JsonPropertyName("violation")]
        public bool Violation { get; set; }
    }

    public class SessionDto
    {
        [JsonPropertyName("suspicious_time")]
        public double SuspiciousTime { get; set; }

        [JsonPropertyName("total_score")]
        public double TotalScore { get; set; }

        [JsonPropertyName("risk_level")]
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class FastApiResponseDto
    {
        [JsonPropertyName("cheating")]
        public bool Cheating { get; set; }

        [JsonPropertyName("student_id")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("exam_id")]
        public string ExamId { get; set; } = string.Empty;

        [JsonPropertyName("phone_detection")]
        public PhoneDetectionDto PhoneDetection { get; set; } = new();

        [JsonPropertyName("person_detection")]
        public PersonDetectionDto PersonDetection { get; set; } = new();

        [JsonPropertyName("eye_tracking")]
        public EyeTrackingDto EyeTracking { get; set; } = new();

        [JsonPropertyName("head_pose")]
        public HeadPoseDto HeadPose { get; set; } = new();

        [JsonPropertyName("face_detection")]
        public FaceDetectionDto FaceDetection { get; set; } = new();

        [JsonPropertyName("current_event")]
        public string CurrentEvent { get; set; } = string.Empty;

        [JsonPropertyName("session")]
        public SessionDto Session { get; set; } = new();

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("inference_time_ms")]
        public int InferenceTimeMs { get; set; }
    }
}
