FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build
WORKDIR /src
COPY . .
RUN dotnet build "./BookstoreWeb/BookstoreWeb.csproj" -c release -o /app/build

FROM build AS publish
RUN dotnet publish "./BookstoreWeb/BookstoreWeb.csproj" -c release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookstoreWeb.dll"]