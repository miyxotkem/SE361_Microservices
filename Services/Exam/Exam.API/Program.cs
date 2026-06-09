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
using Exam.Application.Data;
using Exam.Application.Services;
using Exam.Infrastructure.Data;
using Exam.Infrastructure.Services;
using Course.API.Grpc;
using Identity.API.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Config path for Firebase credential
var pathJson = Path.Combine(builder.Environment.ContentRootPath, "firebase", "firebase_json.json");
if (!File.Exists(pathJson))
{
    var parentFirebase = Path.Combine(Directory.GetCurrentDirectory(), "firebase", "firebase_json.json");
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
    
    return FirestoreDb.Create("e-learning-cd1b3", firestoreClient);
});

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

app.Run();
