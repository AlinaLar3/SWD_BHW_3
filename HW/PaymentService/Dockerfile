# PaymentService/Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY PaymentService/PaymentService.csproj ./PaymentService/
RUN dotnet restore ./PaymentService/PaymentService.csproj

COPY PaymentService/. ./PaymentService/
WORKDIR /src/PaymentService

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]
