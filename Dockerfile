# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Копируем все проекты
COPY . .

# Восстанавливаем зависимости
WORKDIR /app/GraphBackend.Presentation
RUN dotnet restore

# Собираем проект
RUN dotnet publish -c Release -o /out

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

# Указываем порт (по желанию)
EXPOSE 5233
ENTRYPOINT ["dotnet", "GraphBackend.Presentation.dll"]