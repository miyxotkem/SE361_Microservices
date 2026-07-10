/**
 * k6 Load Test - SmartEdu Microservices Scaling Demo
 * 
 * Mô tả: Test tải để chứng minh hiệu quả của Horizontal Scaling
 * - course.api: 3 replicas (Round Robin load balancing qua YARP)
 * - exam.api:   2 replicas (Round Robin load balancing qua YARP)
 * 
 * Chạy: k6 run loadtest.js
 * Chạy với target cụ thể: k6 run --env BASE_URL=http://localhost:7000 loadtest.js
 */

import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

// ─── Custom Metrics ───────────────────────────────────────────────────────────
const courseApiErrors  = new Counter('course_api_errors');
const examApiErrors    = new Counter('exam_api_errors');
const successRate      = new Rate('success_rate');
const courseLatency    = new Trend('course_api_latency', true);
const examLatency      = new Trend('exam_api_latency', true);

// ─── Test Stages (Ramp up → Sustain → Ramp down) ─────────────────────────────
export const options = {
    stages: [
        { duration: '30s', target: 20  },   // Ramp-up: 0 → 20 users
        { duration: '1m',  target: 50  },   // Sustain: 50 concurrent users
        { duration: '30s', target: 100 },   // Stress:  tăng lên 100 users
        { duration: '1m',  target: 100 },   // Sustain: giữ 100 users
        { duration: '30s', target: 0   },   // Ramp-down: về 0
    ],
    thresholds: {
        // 95% request phải dưới 500ms
        http_req_duration:   ['p(95)<500'],
        // Tỉ lệ lỗi phải dưới 1%
        http_req_failed:     ['rate<0.01'],
        // Course API (có 3 replicas) phải nhanh hơn
        course_api_latency:  ['p(95)<300'],
        // Exam API (có 2 replicas)
        exam_api_latency:    ['p(95)<400'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:7000';

export default function () {
    // ── Group 1: Course API (3 replicas - RoundRobin) ────────────────────────
    group('Course Service (3 replicas)', () => {
        const res = http.get(`${BASE_URL}/api/courses`, {
            tags: { service: 'course' },
        });
        courseLatency.add(res.timings.duration);

        const ok = check(res, {
            'course_api: status 200':    (r) => r.status === 200,
            'course_api: response < 1s': (r) => r.timings.duration < 1000,
        });
        if (!ok) courseApiErrors.add(1);
        successRate.add(ok);
    });

    sleep(0.5);

    // ── Group 2: Exam API (2 replicas - RoundRobin) ──────────────────────────
    group('Exam Service (2 replicas)', () => {
        const res = http.get(`${BASE_URL}/api/exams/course/1`, {
            tags: { service: 'exam' },
        });
        examLatency.add(res.timings.duration);

        const ok = check(res, {
            'exam_api: status 2xx or 404': (r) => r.status === 200 || r.status === 404,
            'exam_api: response < 1s':     (r) => r.timings.duration < 1000,
        });
        if (!ok) examApiErrors.add(1);
        successRate.add(ok);
    });

    sleep(0.5);

    // ── Group 3: Identity API (1 replica - baseline) ─────────────────────────
    group('Identity Service (1 replica - baseline)', () => {
        const res = http.get(`${BASE_URL}/api/users`, {
            tags: { service: 'identity' },
        });

        check(res, {
            'identity_api: status 2xx': (r) => r.status === 200 || r.status === 401,
        });
    });

    sleep(1);
}

export function handleSummary(data) {
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
    };
}

function textSummary(data, opts) {
    const { metrics } = data;
    const lines = [
        '',
        '╔══════════════════════════════════════════════════════╗',
        '║         SmartEdu - Scaling Load Test Results         ║',
        '╠══════════════════════════════════════════════════════╣',
        `║  Total Requests  : ${String(metrics.http_reqs?.values?.count ?? '-').padEnd(33)}║`,
        `║  Success Rate    : ${String(((metrics.success_rate?.values?.rate ?? 0) * 100).toFixed(1) + '%').padEnd(33)}║`,
        `║  Avg Duration    : ${String((metrics.http_req_duration?.values?.avg ?? 0).toFixed(1) + 'ms').padEnd(33)}║`,
        `║  p95 Duration    : ${String((metrics.http_req_duration?.values['p(95)'] ?? 0).toFixed(1) + 'ms').padEnd(33)}║`,
        `║  Course p95      : ${String((metrics.course_api_latency?.values['p(95)'] ?? 0).toFixed(1) + 'ms (3 replicas)').padEnd(33)}║`,
        `║  Exam p95        : ${String((metrics.exam_api_latency?.values['p(95)'] ?? 0).toFixed(1) + 'ms (2 replicas)').padEnd(33)}║`,
        '╚══════════════════════════════════════════════════════╝',
        '',
    ];
    return lines.join('\n');
}
