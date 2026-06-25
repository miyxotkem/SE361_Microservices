using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using BuildingBlocks.Security;
using Carter;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Helpers;
using BuildingBlocks.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry Tracing
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "Comment.API");

// Load Firebase Credentials (prioritize environment variable)
GoogleCredential? googleCredential = null;
var firebaseJsonEnv = builder.Configuration["FIREBASE_CREDENTIALS_JSON_COMMENT"];

if (!string.IsNullOrEmpty(firebaseJsonEnv))
{
    googleCredential = GoogleCredential.FromJson(firebaseJsonEnv);
}
else
{
    var pathJson = Path.Combine(builder.Environment.ContentRootPath, "firebase", "firebase_comment.json");
    if (!File.Exists(pathJson))
    {
        var parentFirebase = Path.Combine(Directory.GetCurrentDirectory(), "firebase", "firebase_comment.json");
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
    
    return FirestoreDb.Create("comment-db-10f06", firestoreClient);
});

builder.Services.AddHealthChecks()
    .AddCheck<BuildingBlocks.HealthChecks.FirestoreHealthCheck>("firestore");

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Carter & MediatR
var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

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
