using BuildingBlocks.Messaging.MassTransit;
using MassTransit;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using BuildingBlocks.Security;
using Carter;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Helpers;
using Course.API.Services;
using Quartz;
using BuildingBlocks.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry Tracing
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "Course.API");

// Load Firebase Credentials (prioritize environment variable)
GoogleCredential? googleCredential = null;
var firebaseJsonEnv = builder.Configuration["FIREBASE_CREDENTIALS_JSON_COURSE"];

if (!string.IsNullOrEmpty(firebaseJsonEnv))
{
    googleCredential = GoogleCredential.FromJson(firebaseJsonEnv);
}
else
{
    var pathJson = Path.Combine(builder.Environment.ContentRootPath, "firebase", "firebase_course.json");
    if (!File.Exists(pathJson))
    {
        var parentFirebase = Path.Combine(Directory.GetCurrentDirectory(), "firebase", "firebase_course.json");
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
    
    return FirestoreDb.Create("course-db-28f2a", firestoreClient);
});

builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379", name: "redis")
    .AddCheck<BuildingBlocks.HealthChecks.FirestoreHealthCheck>("firestore")
    .AddRabbitMQ(builder.Configuration);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddGrpc();

// Add Carter & MediatR
var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Add Quartz.NET services
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});

// Add Message Broker (RabbitMQ) for EventBusConsumers
builder.Services.AddMessageBroker(builder.Configuration, assembly, 
    configure: config =>
    {
        config.AddSagaStateMachine<Course.API.Features.Registrations.Sagas.EnrollmentStateMachine, Course.API.Features.Registrations.Sagas.EnrollmentState>()
            .RedisRepository(r =>
            {
                r.DatabaseConfiguration(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379");
            });

        // Add Quartz Scheduler & Consumers to MassTransit
        config.AddPublishMessageScheduler();
        config.AddQuartzConsumers();
    },
    configureBus: (context, cfg) =>
    {
        cfg.UsePublishMessageScheduler();
    });

// Configure JSON serialization to handle Firestore Timestamp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new FirestoreTimestampConverter());
});

// Global Exception Handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddSignalR();

var app = builder.Build();

// Setup Pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.MapGrpcService<CourseGrpcService>();
app.UseExceptionHandler(options => { });
app.MapHub<Course.API.Hubs.EnrollmentHub>("/hubs/enrollment");
app.MapHealthChecks("/health");

app.Run();
