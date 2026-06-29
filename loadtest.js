import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 50 },
        { duration: '1m', target: 50 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.01'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:7000';

export default function () {
    // 1. Course API (Đã có Caching)
    let res = http.get(`${BASE_URL}/api/courses`);
    check(res, { 'course_api 200': (r) => r.status === 200 });

    // 2. Identity API (Chưa có Caching - test toàn bộ user)
    res = http.get(`${BASE_URL}/api/users`);
    check(res, { 'identity_api 200': (r) => r.status === 200 });

    // 3. Comment API (Chưa có Caching)
    res = http.get(`${BASE_URL}/api/comments/lesson/1`);
    check(res, { 'comment_api 200': (r) => r.status === 200 || r.status === 404 }); // Có thể trả về 404 nếu lesson k tồn tại, vẫn tính là pass gateway

    // 4. Exam API (Chưa có Caching)
    res = http.get(`${BASE_URL}/api/exams/course/1`);
    check(res, { 'exam_api 200': (r) => r.status === 200 || r.status === 404 });

    sleep(1);
}
