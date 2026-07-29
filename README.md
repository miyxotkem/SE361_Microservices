<!-- HEADER BANNER -->
<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:11998e,100:38ef7d&height=200&section=header&text=🎓%20SmartEdu%20Microservices&fontSize=50&fontColor=ffffff&desc=Distributed%20Event-Driven%20Platform%20for%20Online%20Education&descAlignY=75" width="100%" alt="SmartEdu Microservices Banner" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9" />
  <img src="https://img.shields.io/badge/Architecture-Microservices-EF6C00?style=for-the-badge&logo=serverless&logoColor=white" alt="Microservices" />
  <img src="https://img.shields.io/badge/Gateway-YARP-673AB7?style=for-the-badge&logo=dotnet&logoColor=white" alt="YARP" />
  <img src="https://img.shields.io/badge/Messaging-RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white" alt="RabbitMQ" />
  <img src="https://img.shields.io/badge/Container-Docker%20%7C%20Kubernetes-2496ED?style=for-the-badge&logo=kubernetes&logoColor=white" alt="Docker/K8s" />
</p>

---

## 🎓 Overview

**SmartEdu (SE361_Microservices)** is a distributed, event-driven online education platform built on **.NET 9**, moving away from a monolithic design toward a full **Microservices Architecture** with the **Database-per-Service** principle. The system combines a **WPF desktop client**, a unified **YARP API Gateway**, six independent business microservices, and an event bus for asynchronous integration — supporting course registration, online exams, real-time notifications, discussions, and online payments end to end.

High-performance internal communication (gRPC), asynchronous event-driven messaging (RabbitMQ + MassTransit), and real-time push updates (SignalR) work together to keep the payment → enrollment → notification pipeline consistent and responsive.

---

## ✨ Key Features

- **Six Independent Microservices:** `Identity`, `Course`, `Exam`, `Payment`, `Notification`, and `Comment` — each a self-contained Bounded Context with its own datastore.
- **Unified API Gateway:** YARP-based gateway handling routing, rate limiting (fixed window, 100 req/10s per client), WebSocket proxying, and distributed tracing for every request.
- **Event-Driven Integration:** RabbitMQ + MassTransit power asynchronous communication between services (payment, enrollment, exam, and notification events).
- **Saga Orchestration:** A Redis-backed `EnrollmentStateMachine` coordinates the payment → enrollment flow, including automatic compensating transactions (refunds) on failure.
- **Real-Time Updates:** SignalR hubs (proxied through the gateway) push instant enrollment and payment status updates to the WPF client.
- **gRPC Internal Communication:** Low-latency internal calls (e.g. checking enrollment eligibility, fetching user profiles, listing course students) between services.
- **Multi-Gateway Payments:** Integrated checkout and webhook/IPN handling for **VNPay**, **MoMo**, and **PayPal**, including voucher discounts and automated refunds.
- **Observability Stack:** OpenTelemetry tracing, Jaeger, and Prometheus metrics wired in via shared Building Blocks for full distributed tracing.
- **Containerized & Cloud-Ready:** Docker Compose for local development and Kubernetes manifests (`k8s/`) for deployment.

---

## 🏛️ System Architecture

The system is organized into four logical layers:

1. **Client Layer:** A WPF desktop application connecting through the API Gateway over HTTPS and WebSockets.
2. **Gateway Layer:** A single YARP API Gateway serving as the sole entry point, routing requests to the correct backend cluster and enforcing rate limiting.
3. **Service Layer:** Six autonomous microservices, each owning its own data and business logic:
   - **Identity.API** — accounts, JWT auth, Google OAuth2, profile sync (PostgreSQL/Supabase).
   - **Course.API** — courses, lessons, enrollment, and the enrollment Saga (Firestore + Redis).
   - **Exam.API** — question banks, exam configuration, auto-grading (Firestore, Clean Architecture + DDD).
   - **Payment.API** — checkout, vouchers, refunds, multi-gateway payment integration (PostgreSQL).
   - **Notification.API** — email and real-time notifications (Firestore).
   - **Comment.API** — lesson discussions and comments (Firestore), isolated so high discussion traffic never affects exams or payments.
4. **Integration & Observability Layer:** RabbitMQ (MassTransit) as the event bus, Redis for Saga/cache state, and OpenTelemetry + Jaeger + Prometheus for distributed tracing and metrics.

### Shared Building Blocks

- **Common:** MediatR pipeline behaviors (`ValidationBehavior`, `LoggingBehavior`), CQRS interfaces, standardized exceptions with `ProblemDetails` handling, OpenTelemetry wiring, and Firestore health checks/helpers.
- **Messaging:** Shared integration commands and events (e.g. `PaymentInitiatedEvent`, `EnrollmentActivatedEvent`, `ExamPublishedEvent`) plus MassTransit/RabbitMQ setup with Quartz-based delayed scheduling.

---

## 🛠️ Technology Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Client** | `C#` / `WPF` | Desktop application for students and faculty. |
| **API Gateway** | `YARP` | Single-entry routing, rate limiting, WebSocket proxying. |
| **Backend Services** | `.NET 9` / `ASP.NET Core` | Six independent microservices (CQRS + MediatR). |
| **Messaging** | `RabbitMQ` / `MassTransit` | Event bus for asynchronous, event-driven integration. |
| **Real-Time** | `SignalR` | Live enrollment and payment status push updates. |
| **Internal RPC** | `gRPC` | Low-latency service-to-service calls. |
| **Relational Data** | `PostgreSQL` (Supabase) | Identity and Payment services. |
| **NoSQL Data** | `Google Cloud Firestore` | Course, Exam, Notification, and Comment services. |
| **State / Cache** | `Redis` | Saga state storage and caching. |
| **Observability** | `OpenTelemetry` / `Jaeger` / `Prometheus` | Distributed tracing and metrics. |
| **Payments** | `VNPay` / `MoMo` / `PayPal` | Payment gateway integrations. |
| **Containerization** | `Docker` / `Kubernetes` | Local orchestration and cloud deployment. |

---

## 📂 Project Structure

Core folders and files in this repository:

*   **`ApiGateways/YarpApiGateway`**: The YARP-based API Gateway — routing, rate limiting, and WebSocket proxying.
*   **`BuildingBlocks`**: Shared cross-cutting code (CQRS, MediatR behaviors, exception handling, messaging contracts, OpenTelemetry setup).
*   **`DTOs` / `Models` / `Helpers`**: Shared data contracts, domain models, and utility logic.
*   **`Services` / `Views`**: WPF client-side services and UI views.
*   **`WebAPI_E_learning` / `firebase`**: Backend API and Firebase/Firestore integration.
*   **`k8s`**: Kubernetes manifests for deployment.
*   **`tests`**: Automated test suites.
*   **`vnpay-test`**: VNPay payment gateway test utilities.
*   **`docker-compose.yml`**: Local multi-service orchestration.
*   **`prometheus.yml` / `otel-collector-config.yaml`**: Observability stack configuration.
*   **`SmartEdu.postman_collection.json`**: Postman collection for API testing.
*   **`architecture_analysis.md`**: In-depth architecture write-up (services, Saga flow, DDD, gRPC/event/SignalR flows).
*   **`HUONG_DAN_TEST.md` / `HUONG_DAN_OBSERVABILITY.md`**: Testing and observability guides.

---

## 🚀 Getting Started

Follow these instructions to set up the project on your local machine for development and testing.

### Prerequisites
*   **Visual Studio 2022** (with .NET desktop development workload) or the **.NET 9 SDK**
*   **Docker Desktop** (for RabbitMQ, Redis, and containerized services via `docker-compose.yml`)
*   **Google Cloud Firestore** project credentials (for Course, Exam, Notification, and Comment services)
*   **PostgreSQL** instance (or Supabase project) for Identity and Payment services
*   **kubectl** (optional, for deploying manifests under `k8s/`)

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/miyxotkem/SE361_Microservices.git
   ```

2. **Start supporting infrastructure:**
   ```bash
   docker-compose up -d
   ```
   This brings up shared infrastructure such as RabbitMQ and Redis.

3. **Configure secrets & credentials:**
   - Add Firestore service account credentials under `firebase/` / `WebAPI_E_learning/firebase/`.
   - Set PostgreSQL/Supabase connection strings for `Identity.API` and `Payment.API`.
   - Configure VNPay / MoMo / PayPal credentials for `Payment.API`.

4. **Run the microservices:**
   - Open `e-learning app.sln` in Visual Studio.
   - Set the `YarpApiGateway` plus the six microservices as startup projects, or run each with `dotnet run` from its project folder.

5. **Run the WPF client:**
   - Set the WPF client project as the startup project and run it against the Gateway's base URL.

6. **Explore the API:**
   - Import `SmartEdu.postman_collection.json` into Postman, or use `api-test.http` directly in Visual Studio / VS Code.

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!
If you would like to contribute:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/YourFeatureName`).
3. Commit your changes (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature/YourFeatureName`).
5. Open a Pull Request.

---

## 👨‍💻 Team & Collaborators

**Thinh Phat Ho**
*Software Engineering Student @ UIT*
* **GitHub:** [@miyxotkem](https://github.com/miyxotkem)
* **Focus:** Full-Stack .NET, Microservices Architecture & API Design
