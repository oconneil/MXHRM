FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MXHRM.Api/MXHRM.Api.csproj", "src/MXHRM.Api/"]
COPY ["src/MXHRM.Application/MXHRM.Application.csproj", "src/MXHRM.Application/"]
COPY ["src/MXHRM.Domain/MXHRM.Domain.csproj", "src/MXHRM.Domain/"]
COPY ["src/MXHRM.Infrastructure/MXHRM.Infrastructure.csproj", "src/MXHRM.Infrastructure/"]

RUN dotnet restore "src/MXHRM.Api/MXHRM.Api.csproj"

COPY . .

RUN dotnet publish "src/MXHRM.Api/MXHRM.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MXHRM.Api.dll"]