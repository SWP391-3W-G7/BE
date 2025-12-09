# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY LostFoundTrackingSystem/LostFoundTrackingSystem.sln ./
COPY LostFoundTrackingSystem/BLL/*.csproj LostFoundTrackingSystem/BLL/
COPY LostFoundTrackingSystem/DAL/*.csproj LostFoundTrackingSystem/DAL/
COPY LostFoundTrackingSystem/LostFoundApi/*.csproj LostFoundTrackingSystem/LostFoundApi/
COPY LostFoundTrackingSystem/LostFoundApi/SwaggerGen/*.csproj LostFoundTrackingSystem/LostFoundApi/SwaggerGen/
RUN dotnet restore "LostFoundTrackingSystem/LostFoundApi/LostFoundApi.csproj"

# Copy the rest of the application files
COPY . .

# Build the application
WORKDIR /src/LostFoundTrackingSystem/LostFoundApi
RUN dotnet build "LostFoundApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LostFoundApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080 # Expose the default HTTP port (or whatever your app uses)

# Copy the published output from the publish stage
COPY --from=publish /app/publish .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "LostFoundApi.dll"]