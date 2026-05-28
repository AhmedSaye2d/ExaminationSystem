"""
Continuous Proctoring Test Script
=================================
Simulates real-time proctoring by sending webcam frames 
to the .NET backend every 1 second.

Usage:
  1. Webcam mode (default):  python test_continuous.py
  2. Video file mode:        python test_continuous.py --video path/to/video.mp4
  3. Single image loop:      python test_continuous.py --image path/to/image.jpg

Options:
  --duration   Total test duration in seconds (default: 30)
  --interval   Seconds between frames (default: 1.0)
  --student-id Student ID (default: 4)
  --exam-id    Exam ID (default: 1)
  --token      JWT Bearer token for authentication
  --base-url   Backend base URL (default: https://localhost:7236)
"""

import argparse
import cv2
import requests
import time
import json
import urllib3
import sys
import os

# Suppress SSL warnings for localhost testing
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)


def get_frame_from_webcam(cap):
    """Capture a single frame from webcam."""
    ret, frame = cap.read()
    if not ret:
        return None
    return frame


def get_frame_from_video(cap):
    """Read next frame from video file."""
    ret, frame = cap.read()
    if not ret:
        # Loop video from the beginning
        cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
        ret, frame = cap.read()
        if not ret:
            return None
    return frame


def get_frame_from_image(image_path):
    """Read frame from a static image file."""
    frame = cv2.imread(image_path)
    return frame


def frame_to_bytes(frame):
    """Encode OpenCV frame to JPEG bytes."""
    _, buffer = cv2.imencode('.jpg', frame)
    return buffer.tobytes()


def send_frame(base_url, token, student_id, exam_id, frame_bytes, frame_num):
    """Send a single frame to the .NET backend."""
    url = f"{base_url}/api/Proctoring/frame"
    
    headers = {
        "accept": "application/json",
    }
    if token:
        headers["Authorization"] = f"Bearer {token}"
    
    files = {
        "Frame": (f"frame_{frame_num}.jpg", frame_bytes, "image/jpeg"),
    }
    data = {
        "StudentId": str(student_id),
        "ExamId": str(exam_id),
    }
    
    try:
        response = requests.post(url, headers=headers, files=files, data=data, verify=False, timeout=30)
        return response.status_code, response.json()
    except requests.exceptions.Timeout:
        return 504, {"error": "Request timed out"}
    except requests.exceptions.ConnectionError as e:
        return 502, {"error": f"Connection failed: {str(e)}"}
    except Exception as e:
        return 500, {"error": str(e)}


def print_response(frame_num, elapsed, status_code, result):
    """Pretty print the AI response."""
    if status_code != 200:
        print(f"  [Frame {frame_num:03d}] [{elapsed:5.1f}s] ERROR {status_code}: {result.get('error', result.get('message', 'Unknown'))}")
        return
    
    cheating = result.get("cheating", False)
    event = result.get("current_event", "none")
    session = result.get("session", {})
    score = session.get("total_score", 0)
    risk = session.get("risk_level", "LOW")
    suspicious = session.get("suspicious_time", 0)
    
    # Get event-specific timer
    timer = 0
    if event == "phone_detected":
        timer = result.get("phone_detection", {}).get("timer_seconds", 0)
    elif event == "extra_person":
        timer = result.get("person_detection", {}).get("timer_seconds", 0)
    elif event == "no_face":
        timer = result.get("face_detection", {}).get("timer_seconds", 0)
    elif event == "gaze_violation":
        timer = result.get("eye_tracking", {}).get("timer_seconds", 0)
    
    # Color coding
    if cheating:
        status_icon = "🚨"
    else:
        status_icon = "✅"
    
    print(f"  {status_icon} [Frame {frame_num:03d}] [{elapsed:5.1f}s] "
          f"Event: {event:<20s} Timer: {timer:5.1f}s  "
          f"Score: {score:5.1f}  Risk: {risk:<12s} "
          f"Suspicious: {suspicious:5.1f}s  Cheating: {cheating}")


def main():
    parser = argparse.ArgumentParser(description="Continuous Proctoring Test")
    parser.add_argument("--video", type=str, help="Path to video file")
    parser.add_argument("--image", type=str, help="Path to single image (will be sent repeatedly)")
    parser.add_argument("--duration", type=int, default=30, help="Test duration in seconds (default: 30)")
    parser.add_argument("--interval", type=float, default=1.0, help="Seconds between frames (default: 1.0)")
    parser.add_argument("--student-id", type=int, default=4, help="Student ID (default: 4)")
    parser.add_argument("--exam-id", type=int, default=1, help="Exam ID (default: 1)")
    parser.add_argument("--token", type=str, default="", help="JWT Bearer token")
    parser.add_argument("--base-url", type=str, default="https://localhost:7236", help="Backend base URL")
    args = parser.parse_args()

    print("=" * 70)
    print("       CONTINUOUS PROCTORING TEST")
    print("=" * 70)
    print(f"  Backend URL : {args.base_url}")
    print(f"  Student ID  : {args.student_id}")
    print(f"  Exam ID     : {args.exam_id}")
    print(f"  Duration    : {args.duration}s")
    print(f"  Interval    : {args.interval}s")
    
    cap = None
    source_mode = ""
    
    if args.video:
        if not os.path.exists(args.video):
            print(f"\n  ERROR: Video file not found: {args.video}")
            sys.exit(1)
        cap = cv2.VideoCapture(args.video)
        source_mode = f"Video: {args.video}"
    elif args.image:
        if not os.path.exists(args.image):
            print(f"\n  ERROR: Image file not found: {args.image}")
            sys.exit(1)
        source_mode = f"Image: {args.image}"
    else:
        cap = cv2.VideoCapture(0)
        if not cap.isOpened():
            print("\n  ERROR: Could not open webcam. Use --image or --video instead.")
            sys.exit(1)
        source_mode = "Webcam (live)"
    
    print(f"  Source      : {source_mode}")
    print(f"  Token       : {'Set' if args.token else 'NOT SET (will likely get 401)'}")
    print("=" * 70)
    
    if not args.token:
        print("\n  ⚠️  WARNING: No --token provided. Requests will likely fail with 401.")
        print("     Get a token from: POST /api/Auth/login")
        print("     Then run: python test_continuous.py --token YOUR_TOKEN\n")
    
    print("\n  Starting in 2 seconds... (Ctrl+C to stop)\n")
    time.sleep(2)
    
    start_time = time.time()
    frame_num = 0
    cheating_count = 0
    
    try:
        while (time.time() - start_time) < args.duration:
            frame_num += 1
            elapsed = time.time() - start_time
            
            # Get frame based on source
            if args.image:
                frame = get_frame_from_image(args.image)
            elif args.video:
                frame = get_frame_from_video(cap)
            else:
                frame = get_frame_from_webcam(cap)
            
            if frame is None:
                print(f"  [Frame {frame_num:03d}] Failed to capture frame. Skipping...")
                time.sleep(args.interval)
                continue
            
            # Encode and send
            frame_bytes = frame_to_bytes(frame)
            status_code, result = send_frame(
                args.base_url, args.token, 
                args.student_id, args.exam_id, 
                frame_bytes, frame_num
            )
            
            print_response(frame_num, elapsed, status_code, result)
            
            if status_code == 200 and result.get("cheating", False):
                cheating_count += 1
            
            # Wait for next interval
            next_time = start_time + (frame_num * args.interval)
            sleep_time = next_time - time.time()
            if sleep_time > 0:
                time.sleep(sleep_time)
    
    except KeyboardInterrupt:
        print("\n\n  ⏹️  Test stopped by user.")
    
    finally:
        if cap is not None:
            cap.release()
    
    total_time = time.time() - start_time
    print("\n" + "=" * 70)
    print("       TEST SUMMARY")
    print("=" * 70)
    print(f"  Total Frames Sent    : {frame_num}")
    print(f"  Total Time           : {total_time:.1f}s")
    print(f"  Cheating Detections  : {cheating_count}")
    print(f"  Detection Rate       : {(cheating_count/max(frame_num,1))*100:.1f}%")
    print("=" * 70)
    print("\n  Now generate the Excel report to verify the data!")
    print("  GET /api/Report/exam/{exam_id}/excel\n")


if __name__ == "__main__":
    main()
