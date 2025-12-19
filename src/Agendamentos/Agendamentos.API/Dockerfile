# Build and publish Agendamentos API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Agendamentos/Agendamentos.API/Agendamentos.API.csproj
RUN dotnet publish src/Agendamentos/Agendamentos.API/Agendamentos.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","Agendamentos.API.dll"]
