# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY ["azure-online-survey-saas.sln", "./"]
COPY ["backend/src/SurveyApi.Domain/SurveyApi.Domain.csproj", "backend/src/SurveyApi.Domain/"]
COPY ["backend/src/SurveyApi.Application/SurveyApi.Application.csproj", "backend/src/SurveyApi.Application/"]
COPY ["backend/src/SurveyApi.Infrastructure/SurveyApi.Infrastructure.csproj", "backend/src/SurveyApi.Infrastructure/"]
COPY ["backend/src/SurveyApi.Web/SurveyApi.Web.csproj", "backend/src/SurveyApi.Web/"]

# Restore all projects
RUN dotnet restore "backend/src/SurveyApi.Web/SurveyApi.Web.csproj"

# Copy the rest of the source code
COPY . ./

# Publish the Web project
RUN dotnet publish "backend/src/SurveyApi.Web/SurveyApi.Web.csproj" -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
ENTRYPOINT ["dotnet", "SurveyApi.Web.dll"]
