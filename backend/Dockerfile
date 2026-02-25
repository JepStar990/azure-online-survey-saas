FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["SurveyApi.csproj", "./"]
RUN dotnet restore "SurveyApi.csproj"

# copy the rest and publish
COPY . ./
RUN dotnet publish "SurveyApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "SurveyApi.dll"]
