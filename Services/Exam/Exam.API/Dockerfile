FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Services/Exam/Exam.API/Exam.API.csproj", "Services/Exam/Exam.API/"]
COPY ["Services/Exam/Exam.Domain/Exam.Domain.csproj", "Services/Exam/Exam.Domain/"]
COPY ["Services/Exam/Exam.Application/Exam.Application.csproj", "Services/Exam/Exam.Application/"]
COPY ["Services/Exam/Exam.Infrastructure/Exam.Infrastructure.csproj", "Services/Exam/Exam.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks/BuildingBlocks.Messaging/"]

RUN dotnet restore "Services/Exam/Exam.API/Exam.API.csproj"
COPY . .
WORKDIR "/src/Services/Exam/Exam.API"
RUN dotnet build "Exam.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Exam.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Exam.API.dll"]
