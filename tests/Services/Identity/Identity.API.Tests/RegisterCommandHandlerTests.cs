using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.API.Data;
using Identity.API.Features.Auth.Register;
using Identity.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.API.Tests;

public class RegisterCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IdentityDbContext _context;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new IdentityDbContext(options);
        _context.Database.EnsureCreated();

        _handler = new RegisterCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUserSuccessfully_WhenEmailIsUnique()
    {
        // Arrange
        var command = new RegisterCommand("newuser@gmail.com", "Password123!", "John Doe");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Since RegisterCommandHandler returns a Carter IResult (Ok Object Result)
        // We can inspect the status code via a dummy HTTP context if needed, 
        // or since it is IResult, we can check database directly to verify addition.
        
        var userInDb = await _context.Users.SingleOrDefaultAsync(u => u.Email == "newuser@gmail.com");
        userInDb.Should().NotBeNull();
        userInDb!.FullName.Should().Be("John Doe");
        userInDb.Role.Should().Be("Student");
        userInDb.IsBlocked.Should().BeFalse();
        BCrypt.Net.BCrypt.Verify("Password123!", userInDb.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "existinguser@gmail.com",
            PasswordHash = "somehash",
            FullName = "Existing User",
            Role = "Student"
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var command = new RegisterCommand("ExistingUser@gmail.com", "Password123!", "Duplicate User");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Database count of this email should still be 1
        var count = await _context.Users.CountAsync(u => u.Email.ToLower() == "existinguser@gmail.com");
        count.Should().Be(1);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
