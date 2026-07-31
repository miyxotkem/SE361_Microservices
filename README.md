<!-- HEADER BANNER -->
<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:11998e,100:38ef7d&height=200&section=header&text=🎓%20SmartEdu%20Microservices&fontSize=48&fontColor=ffffff&desc=Cloud-Native%20E-Learning%20Platform%20on%20.NET%209&descAlignY=75" width="100%" alt="SmartEdu Microservices Banner" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9" />
  <img src="https://img.shields.io/badge/Services-7%20Microservices-EF6C00?style=for-the-badge&logo=serverless&logoColor=white" alt="7 Microservices" />
  <img src="https://img.shields.io/badge/Gateway-YARP-673AB7?style=for-the-badge&logo=dotnet&logoColor=white" alt="YARP" />
  <img src="https://img.shields.io/badge/Messaging-RabbitMQ%20%2F%20MassTransit-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white" alt="RabbitMQ" />
  <img src="https://img.shields.io/badge/Container-Docker%20%7C%20Kubernetes-2496ED?style=for-the-badge&logo=kubernetes&logoColor=white" alt="Docker/K8s" />
</p>

---

## 🎓 Overview

**SmartEdu (SE361_Microservices)** is the microservices evolution of the SE104 E-learning System (SmartEdu), rebuilt on **.NET 9** as **7 independently deployable services** behind a unified **YARP API Gateway**, with the same **WPF desktop client** as the front end. It keeps the same domain (course registration, learning content, assignments, online exams, notifications) and adds a full commerce layer: multi-gateway payments, a Saga-orchestrated enrollment flow, and production-grade observability.

---

## ✨ Key Features

- **7 Independent Microservices**, each with its own datastore: `Identity`, `Course`, `Exam`, `Payment`, `Notification`, `Comment`, and `Admin`.
- **CQRS + Vertical Slice Architecture:** Every business capability is a self-contained `Features/<Area>/<Action>` folder with a paired `Endpoint` + `Handler` (Course, Exam, Notification, Comment, Identity services), using MediatR pipelines with shared `ValidationBehavior`, `LoggingBehavior`, and `CachingBehavior`.
- **Clean Architecture for Exam Service:** `Exam.API` / `Exam.Application` / `Exam.Domain` / `Exam.Infrastructure` layering with its own commands/queries and repository abstraction.
- **Unified API Gateway (YARP):** Single entry point routing to all services, with rate limiting, WebSocket proxying, and Docker/K8s deployment configs.
- **Event-Driven Integration:** RabbitMQ + MassTransit power async communication — `PaymentInitiatedEvent`, `PaymentCompletedEvent`, `EnrollmentActivatedEvent`, `EnrollmentFailedEvent`, `ExamPublishedEvent`, `CoursePurchasedEvent`, and commands like `UpdateEnrollmentCommand` / `RefundPaymentCommand`.
- **Saga Orchestration:** `EnrollmentStateMachine` (`Course.API/Features/Registrations/Sagas`) coordinates the payment → enrollment pipeline, including automatic refunds via `RefundPaymentConsumer` on `Payment.API`.
- **gRPC Internal Communication:** `Identity.API` and `Course.API` expose gRPC services (`user.proto`, `course.proto`) for low-latency internal calls, consumed by `Exam.Infrastructure`'s `UserServiceClient` / `CourseServiceClient`.
- **Real-Time Updates:** `EnrollmentHub` (SignalR) on `Course.API`, proxied through the gateway, pushes live enrollment/payment status to the WPF client.
- **Multi-Gateway Payments:** `Payment.API` integrates **VNPay**, **MoMo**, and **PayPal** (`VnPayService`, `MoMoService`, `PayPalService`) behind a common `IPaymentGatewayService`, backed by EF Core migrations on its own database.
- **Identity with EF Core + gRPC:** `Identity.API` handles registration, email login, Google login, forgot password, avatar management, role changes, and user blocking, with its own `IdentityDbContext` and migrations.
- **Observability Stack:** OpenTelemetry (`OpenTelemetryExtensions`), Jaeger, Prometheus (`prometheus.yml`, `otel-collector-config.yaml`) wired via shared `BuildingBlocks` health checks (`FirestoreHealthCheck`).
- **Testing:** Dedicated unit + integration test projects per service under `tests/Services/<Service>` (Comment, Course, Exam, Identity, Notification, Payment) plus `BuildingBlocks.Tests`.
- **CI/CD:** GitHub Actions workflows for backend CI and CD (`.github/workflows/ci-backend.yml`, `cd-backend.yml`).
- **Load & API Testing:** `loadtest.js`, `test-auth.js`, `api-test.http`, and a full Postman collection (`SmartEdu.postman_collection.json`).

---

## 🏛️ System Architecture

1. **Client Layer:** The WPF desktop app (`Views/Student`, `Views/Teacher`, `Views/Admin`, `Views/Payment`, `Views/Common`) connects through the API Gateway over HTTPS/WebSockets.
2. **Gateway Layer:** `ApiGateways/YarpApiGateway` — the sole entry point, routing to each service cluster.
3. **Service Layer:**
   - **Identity.API** — accounts, JWT auth, Google OAuth2, roles, avatars (EF Core + gRPC).
   - **Course.API** — courses, lessons, content, assignments, registrations, and the enrollment Saga (+ gRPC + SignalR).
   - **Exam.API** — exams, drafts, questions, submissions, grading history (Clean Architecture, 4-project split).
   - **Payment.API** — checkout, refunds, VNPay/MoMo/PayPal integration (EF Core).
   - **Notification.API** — event-driven notifications, consuming `ExamPublishedEvent` and `EnrollmentActivatedEvent`.
   - **Comment.API** — lesson/course discussion comments, isolated from the rest.
   - **Admin.API** — dashboard statistics for administrators.
4. **Integration & Observability Layer:** RabbitMQ/MassTransit event bus, Redis, Jaeger, Prometheus, OpenTelemetry Collector.

### Shared Building Blocks (`BuildingBlocks/`)
- **`BuildingBlocks`:** CQRS interfaces (`ICommand`, `IQuery`, handlers), MediatR behaviors, custom exceptions with `CustomExceptionHandler`, pagination helpers, JWT extensions, Firestore timestamp conversion, and OpenTelemetry setup.
- **`BuildingBlocks.Messaging`:** Shared integration events/commands and MassTransit + RabbitMQ configuration extensions.

---

## 🛠️ Technology Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Client** | `C#` / `WPF` (.NET 9) | Desktop app shared with the SE104 monolith. |
| **API Gateway** | `YARP` | Routing, rate limiting, WebSocket proxying. |
| **Backend Services** | `.NET 9` / `ASP.NET Core` | 7 microservices, CQRS + MediatR, some with Clean Architecture. |
| **Messaging** | `RabbitMQ` / `MassTransit` | Event bus + Saga orchestration. |
| **Real-Time** | `SignalR` | `EnrollmentHub` for live status updates. |
| **Internal RPC** | `gRPC` | Identity and Course service-to-service calls. |
| **Relational Data** | `PostgreSQL` (EF Core) | Identity and Payment services, with migrations. |
| **NoSQL Data** | `Google Cloud Firestore` | Course, Exam, Notification, Comment services. |
| **Cache / State** | `Redis` | Saga state and caching. |
| **Observability** | `OpenTelemetry` / `Jaeger` / `Prometheus` | Distributed tracing and metrics. |
| **Payments** | `VNPay` / `MoMo` / `PayPal` | Multi-gateway payment integration. |
| **Containerization** | `Docker` / `Kubernetes` | Per-service Dockerfiles + `k8s/` manifests (incl. HPA autoscaling). |
| **CI/CD** | `GitHub Actions` | Backend CI and CD pipelines. |

---

## 📂 Project Structure

*   **`ApiGateways/YarpApiGateway`**: YARP gateway service.
*   **`BuildingBlocks/BuildingBlocks`, `BuildingBlocks/BuildingBlocks.Messaging`**: Shared cross-cutting libraries.
*   **`Services/Identity`, `Services/Course`, `Services/Exam`, `Services/Payment`, `Services/Notification`, `Services/Comment`, `Services/Admin`**: The 7 microservices, each with its own `Program.cs`, `Dockerfile`, `appsettings.*.json`, and `Features/`/`Endpoints/` folders.
*   **`Views/*`, `Models`, `DTOs`, `Helpers`, `Services/*.cs`** (root-level): The shared WPF client, reusing the same client architecture as the SE104 monolith.
*   **`tests/`**: Unit and integration tests per service, plus `BuildingBlocks.Tests`.
*   **`k8s/`**: Kubernetes manifests — per-service deployments, `rabbitmq.yaml`, `redis.yaml`, `jaeger.yaml`, `gateway.yaml`, `hpa.yaml`, `secrets.yaml`.
*   **`Documents/`**: Full academic deliverable set (Vision & Scope, Use Cases, Business Rules, SRS, SDD, Requirements, Features Backlog, Test Suite) — shared lineage with the SE104 monolith.
*   **`docker-compose.yml`**: Local orchestration for RabbitMQ, Redis, Jaeger, the gateway, and all services.
*   **`prometheus.yml` / `otel-collector-config.yaml`**: Observability stack configuration.
*   **`SmartEdu.postman_collection.json`**, **`api-test.http`**: API testing collections.
*   **`loadtest.js`**, **`test-auth.js`**: Load and auth testing scripts.
*   **`architecture_analysis.md`**: In-depth architecture write-up (services, Saga flow, DDD, gRPC/event/SignalR flows).
*   **`HUONG_DAN_TEST.md`**, **`HUONG_DAN_OBSERVABILITY.md`**: Testing and observability guides.
*   **`.github/workflows/`**: CI (`ci-backend.yml`) and CD (`cd-backend.yml`) pipelines.

---

## 🚀 Getting Started

### Prerequisites
*   **Visual Studio 2022** (with .NET desktop development workload) or the **.NET 9 SDK**
*   **Docker Desktop** (for RabbitMQ, Redis, Jaeger, and all services via `docker-compose.yml`)
*   **Google Cloud Firestore** credentials (Course, Exam, Notification, Comment services)
*   **PostgreSQL** (or a container) for Identity and Payment services' EF Core databases
*   **kubectl** (optional, for deploying `k8s/` manifests)

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/miyxotkem/SE361_Microservices.git
   ```

2. **Start supporting infrastructure:**
   ```bash
   docker-compose up -d
   ```

3. **Configure secrets & credentials:**
   - Add Firestore service account credentials under `firebase/` (used by Course/Exam/Notification/Comment).
   - Set PostgreSQL connection strings for `Identity.API` and `Payment.API`, then run EF Core migrations:
     ```bash
     dotnet ef database update --project Services/Identity/Identity.API
     dotnet ef database update --project Services/Payment/Payment.API
     ```
   - Configure VNPay / MoMo / PayPal credentials in `Services/Payment/Payment.API/appsettings.json`.

4. **Run the microservices:**
   - Open `e-learning app.sln` in Visual Studio.
   - Set `YarpApiGateway` plus the 7 services as startup projects, or run each with `dotnet run` from its project folder.

5. **Run the WPF client** against the Gateway's base URL.

6. **Explore the API:** import `SmartEdu.postman_collection.json` into Postman, or use `api-test.http` directly.

7. **Run tests:**
   ```bash
   dotnet test
   ```

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/YourFeatureName`).
3. Commit your changes (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature/YourFeatureName`).
5. Open a Pull Request.

---

## 👨‍💻 Team & Collaborators

**Võ Tấn Nhã**  
*Software Engineering Student @ UIT*
* **GitHub:** [@nha-blip](https://github.com/nha-blip)
* **Focus:** Full-Stack .NET, Microservices Architecture & API Design

**Thinh Phat Ho**  
*Software Engineering Student @ UIT*
* **GitHub:** [@miyxotkem](https://github.com/miyxotkem)
* **Focus:** Full-Stack .NET, Microservices Architecture & API Design

**Đinh Quang Nhật**  
*Software Engineering Student @ UIT*
* **GitHub:** [@PeterBrr](https://github.com/PeterBrr)
* **Focus:** Full-Stack .NET, Microservices Architecture & API Design

**innguyen**  
*Software Engineering Student @ UIT*
* **GitHub:** [@innguyen](https://github.com/innguyen)
* **Focus:** Full-Stack .NET, Microservices Architecture & API Design
