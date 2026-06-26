using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using BuildingBlocks.Security;
using Carter;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Helpers;
using BuildingBlocks.Messaging.MassTransit;
using Exam.Application.Data;
using Exam.Application.Services;
using Exam.Infrastructure.Data;
using Exam.Infrastructure.Services;
using Course.API.Grpc;
using Identity.API.Grpc;
using BuildingBlocks.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry Tracing
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "Exam.API");

// Load Firebase Credentials (prioritize environment variable)
GoogleCredential? googleCredential = null;
var firebaseJsonEnv = builder.Configuration["FIREBASE_CREDENTIALS_JSON_EXAM"];

if (!string.IsNullOrEmpty(firebaseJsonEnv))
{
    googleCredential = GoogleCredential.FromJson(firebaseJsonEnv);
}
else
{
    var pathJson = Path.Combine(builder.Environment.ContentRootPath, "firebase", "firebase_exam.json");
    if (!File.Exists(pathJson))
    {
        var parentFirebase = Path.Combine(Directory.GetCurrentDirectory(), "firebase", "firebase_exam.json");
        if (File.Exists(parentFirebase))
        {
            pathJson = parentFirebase;
        }
    }
    if (File.Exists(pathJson))
    {
        googleCredential = GoogleCredential.FromFile(pathJson);
    }
}

// Initialize Firebase Admin
if (googleCredential != null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = googleCredential
    });
}

// Add Firestore to DI
builder.Services.AddSingleton(provider =>
{
    var credential = googleCredential ?? GoogleCredential.GetApplicationDefault();
    var firestoreClient = new Google.Cloud.Firestore.V1.FirestoreClientBuilder
    {
        Credential = credential
    }.Build();
    
    return FirestoreDb.Create("exam-db-8e1b4", firestoreClient);
});

builder.Services.AddHealthChecks()
    .AddCheck<BuildingBlocks.HealthChecks.FirestoreHealthCheck>("firestore")
    .AddRabbitMQ(builder.Configuration);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Carter & MediatR
var appAssembly = typeof(IExamRepository).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(appAssembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Register Exam Repository and Service Clients
builder.Services.AddScoped<IExamRepository, ExamRepository>();

// Register gRPC Client and Wrappers
builder.Services.AddGrpcClient<CourseProtoService.CourseProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:CourseUrl"] ?? "http://localhost:7002");
});
builder.Services.AddGrpcClient<UserProtoService.UserProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:IdentityUrl"] ?? "http://localhost:7001");
});

builder.Services.AddScoped<ICourseServiceClient, CourseServiceClient>();
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();

// Add Message Broker (RabbitMQ via MassTransit)
builder.Services.AddMessageBroker(builder.Configuration, appAssembly);

// Configure JSON serialization to handle Firestore Timestamp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new FirestoreTimestampConverter());
});

// Global Exception Handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Setup Pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.UseExceptionHandler(options => { });
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
