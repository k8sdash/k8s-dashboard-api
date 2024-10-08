#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["K8SDashboard,Api/K8SDashboard,Api.csproj", "K8SDashboard,Api/"]
RUN dotnet restore "K8SDashboard,Api/K8SDashboard,Api.csproj"
COPY . .
WORKDIR "/src/K8SDashboard,Api"
RUN dotnet build "K8SDashboard,Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "K8SDashboard,Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "K8SDashboard,Api.dll"]