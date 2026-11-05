
# Use ASP.NET Core 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set working directory
WORKDIR /app

# Copy all published application files
COPY . .

# Expose application port
EXPOSE 8080

# Configure ASP.NET Core to listen on port 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Start application
ENTRYPOINT ["dotnet", "Inventory.dll"]
