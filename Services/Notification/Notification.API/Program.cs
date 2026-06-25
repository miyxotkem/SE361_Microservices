using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Carter;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Helpers;
using BuildingBlocks.Messaging.MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Config path for Firebase credential
var pathJson = Path.Combine(builder.Environment.ContentRootPath, "firebase", "firebase_notification.json");
if (!File.Exists(pathJson))
{
    var parentFirebase = Path.Combine(Directory.GetCurrentDirectory(), "firebase", "firebase_notification.json");
    if (File.Exists(parentFirebase))
    {
        pathJson = parentFirebase;
    }
}

// Initialize Firebase Admin
if (File.Exists(pathJson))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(pathJson)
    });
}

// Add Firestore to DI
builder.Services.AddSingleton(provider =>
{
    GoogleCredential credential;
    if (File.Exists(pathJson))
    {
        credential = GoogleCredential.FromFile(pathJson);
    }
    else
    {
        credential = GoogleCredential.GetApplicationDefault();
    }

    var firestoreClient = new Google.Cloud.Firestore.V1.FirestoreClientBuilder
    {
        Credential = credential
    }.Build();
    
    return FirestoreDb.Create("notification-db-9b061", firestoreClient);
});

builder.Services.AddHealthChecks()
    .AddCheck<BuildingBlocks.HealthChecks.FirestoreHealthCheck>("firestore");

// Add JWT Authentication
var jwtkey = builder.Configuration["Jwt:Key"] ?? "super_secret_key_smartedu_1234567890";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartEdu",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SmartEduClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtkey))
        };
    });

builder.Services.AddAuthorization();

// Register gRPC Client for CourseProtoService
builder.Services.AddGrpcClient<Course.API.Grpc.CourseProtoService.CourseProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:CourseUrl"] ?? "http://localhost:7002");
});

// Add Carter & MediatR
var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Add Message Broker (RabbitMQ via MassTransit, registering consumers in this assembly)
builder.Services.AddMessageBroker(builder.Configuration, assembly);

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
