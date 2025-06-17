# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy your C# project and restore
COPY UDPTester.csproj ./
RUN dotnet restore

# Copy the source code and build
COPY . ./
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .

# Run the tester app
ENTRYPOINT ["dotnet", "UDPTester.dll"]