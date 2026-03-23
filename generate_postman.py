import json
import os

collection = {
    "info": {
        "name": "Examination System API (With DbSeeder Examples)",
        "description": "Comprehensive Postman Collection for the Examination System, complete with example data based on DbSeeder.",
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    "variable": [
        {"key": "baseUrl", "value": "http://localhost:5239" },
        {"key": "token", "value": "" }
    ],
    "item": [
        {
            "name": "1. Authentication",
            "item": [
                {
                    "name": "Login (Admin)",
                    "event": [{"listen": "test", "script": { "exec": ["var jsonData = pm.response.json();", "pm.environment.set('token', jsonData.token);"], "type": "text/javascript" }}],
                    "request": {
                        "method": "POST",
                        "header": [{"key": "Content-Type", "value": "application/json"}],
                        "body": {"mode": "raw", "raw": '{\n  "email": "admin@example.com",\n  "password": "P@ssword123"\n}'},
                        "url": {"raw": "{{baseUrl}}/api/auth/login", "host": ["{{baseUrl}}"], "path": ["api", "auth", "login"]}
                    }
                },
                {
                    "name": "Login (Instructor: sara)",
                    "event": [{"listen": "test", "script": { "exec": ["var jsonData = pm.response.json();", "pm.environment.set('token', jsonData.token);"], "type": "text/javascript" }}],
                    "request": {
                        "method": "POST",
                        "header": [{"key": "Content-Type", "value": "application/json"}],
                        "body": {"mode": "raw", "raw": '{\n  "email": "sara@exam.com",\n  "password": "P@ssword123"\n}'},
                        "url": {"raw": "{{baseUrl}}/api/auth/login", "host": ["{{baseUrl}}"], "path": ["api", "auth", "login"]}
                    }
                },
                {
                    "name": "Login (Student: postman)",
                    "event": [{"listen": "test", "script": { "exec": ["var jsonData = pm.response.json();", "pm.environment.set('token', jsonData.token);"], "type": "text/javascript" }}],
                    "request": {
                        "method": "POST",
                        "header": [{"key": "Content-Type", "value": "application/json"}],
                        "body": {"mode": "raw", "raw": '{\n  "email": "postman@example.com",\n  "password": "P@ssword123"\n}'},
                        "url": {"raw": "{{baseUrl}}/api/auth/login", "host": ["{{baseUrl}}"], "path": ["api", "auth", "login"]}
                    }
                }
            ]
        },
        {
            "name": "2. Admin Dashboard",
            "item": [
                {
                    "name": "Get Stats",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/admin/dashboard/stats", "host": ["{{baseUrl}}"], "path": ["api", "admin", "dashboard", "stats"]}
                    }
                }
            ]
        },
        {
            "name": "3. Departments",
            "item": [
                {
                    "name": "Get All Departments",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/departments/GetAll", "host": ["{{baseUrl}}"], "path": ["api", "departments", "GetAll"]}
                    }
                },
                {
                    "name": "Create Department",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "POST",
                        "header": [{"key": "Content-Type", "value": "application/json"}],
                        "body": {"mode": "raw", "raw": '{\n  "name": "Artificial Intelligence",\n  "code": "AI"\n}'},
                        "url": {"raw": "{{baseUrl}}/api/departments/Create", "host": ["{{baseUrl}}"], "path": ["api", "departments", "Create"]}
                    }
                }
            ]
        },
        {
            "name": "4. Courses",
            "item": [
                {
                    "name": "Get All Courses",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/courses/GetAll", "host": ["{{baseUrl}}"], "path": ["api", "courses", "GetAll"]}
                    }
                },
                {
                    "name": "Assign Instructor",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "POST",
                        "url": {"raw": "{{baseUrl}}/api/courses/1/assign-instructor?instructorId=1", "host": ["{{baseUrl}}"], "path": ["api", "courses", "1", "assign-instructor"], "query": [{"key": "instructorId", "value": "1"}]}
                    }
                }
            ]
        },
        {
            "name": "5. Students",
            "item": [
                {
                    "name": "Get All Students",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/students/GetAll", "host": ["{{baseUrl}}"], "path": ["api", "students", "GetAll"]}
                    }
                },
                {
                    "name": "Get My Courses",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/students/my-courses", "host": ["{{baseUrl}}"], "path": ["api", "students", "my-courses"]}
                    }
                },
                {
                    "name": "Enroll In Course",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "POST",
                        "url": {"raw": "{{baseUrl}}/api/students/enroll?courseId=1", "host": ["{{baseUrl}}"], "path": ["api", "students", "enroll"], "query": [{"key": "courseId", "value": "1"}]}
                    }
                },
                {
                    "name": "Get My Exams",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/students/my-exams", "host": ["{{baseUrl}}"], "path": ["api", "students", "my-exams"]}
                    }
                },
                {
                    "name": "Get My Results",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/students/my-results", "host": ["{{baseUrl}}"], "path": ["api", "students", "my-results"]}
                    }
                }
            ]
        },
        {
            "name": "6. Exams",
            "item": [
                {
                    "name": "Get All Exams",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/exams/GetAll", "host": ["{{baseUrl}}"], "path": ["api", "exams", "GetAll"]}
                    }
                },
                {
                    "name": "Get Instructor Exams",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/exams/my-exams", "host": ["{{baseUrl}}"], "path": ["api", "exams", "my-exams"]}
                    }
                },
                {
                    "name": "Create Exam",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "POST",
                        "header": [{"key": "Content-Type", "value": "application/json"}],
                        "body": {"mode": "raw", "raw": '{\n  "name": "Testing Exam",\n  "description": "Mock Data",\n  "startDate": "2026-03-30T00:00:00Z",\n  "dueDate": "2026-04-05T00:00:00Z",\n  "totalQuestions": 3,\n  "totalPoints": 30,\n  "type": 0,\n  "courseId": 1,\n  "settings": {\n    "shuffleQuestions": true,\n    "showResultsAfterSubmit": true\n  }\n}'},
                        "url": {"raw": "{{baseUrl}}/api/exams/Create", "host": ["{{baseUrl}}"], "path": ["api", "exams", "Create"]}
                    }
                }
            ]
        },
        {
            "name": "7. Questions & Choices",
            "item": [
                {
                    "name": "Get All Questions",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/questions/GetAll", "host": ["{{baseUrl}}"], "path": ["api", "questions", "GetAll"]}
                    }
                },
                {
                    "name": "Create Question with Choices",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "POST",
                        "header": [{"key": "Content-Type", "value": "application/json"}],
                        "body": {"mode": "raw", "raw": '{\n  "examId": 1,\n  "text": "What is 2+2?",\n  "type": 0,\n  "difficultyLevel": 1,\n  "grade": 5,\n  "choices": [\n    {"text": "4", "isCorrectAnswer": true, "order": 1},\n    {"text": "5", "isCorrectAnswer": false, "order": 2}\n  ]\n}'},
                        "url": {"raw": "{{baseUrl}}/api/questions/with-choices", "host": ["{{baseUrl}}"], "path": ["api", "questions", "with-choices"]}
                    }
                }
            ]
        },
        {
            "name": "8. Student Exams Pipeline",
            "item": [
                {
                    "name": "Start Exam",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "POST",
                        "url": {"raw": "{{baseUrl}}/api/student-exams/start?examId=1", "host": ["{{baseUrl}}"], "path": ["api", "student-exams", "start"], "query": [{"key": "examId", "value": "1"}]}
                    }
                },
                {
                    "name": "Get Active Questions",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/student-exams/1/questions", "host": ["{{baseUrl}}"], "path": ["api", "student-exams", "1", "questions"]}
                    }
                },
                {
                    "name": "Save Answer",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "POST",
                        "header": [{"key": "Content-Type", "value": "application/json"}],
                        "body": {"mode": "raw", "raw": '{\n  "questionId": 1,\n  "choiceId": 2\n}'},
                        "url": {"raw": "{{baseUrl}}/api/student-exams/1/answers", "host": ["{{baseUrl}}"], "path": ["api", "student-exams", "1", "answers"]}
                    }
                },
                {
                    "name": "Submit Exam",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "POST",
                        "url": {"raw": "{{baseUrl}}/api/student-exams/1/submit", "host": ["{{baseUrl}}"], "path": ["api", "student-exams", "1", "submit"]}
                    }
                },
                {
                    "name": "Get Final Result",
                    "request": {
                        "auth": {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]},
                        "method": "GET",
                        "url": {"raw": "{{baseUrl}}/api/student-exams/1/result", "host": ["{{baseUrl}}"], "path": ["api", "student-exams", "1", "result"]}
                    }
                }
            ]
        }
    ]
}

with open("ExaminationSystem_PostmanCollection.json", "w") as f:
    json.dump(collection, f, indent=4)
print("done")
