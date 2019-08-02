FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["TestWebApp/TestWebApp.csproj", "TestWebApp/"]
RUN dotnet restore "TestWebApp/TestWebApp.csproj"
COPY . .
WORKDIR "/src/TestWebApp"
RUN dotnet build "TestWebApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TestWebApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TestWebApp.dll"]