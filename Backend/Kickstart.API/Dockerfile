FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Kickstart.API/Kickstart.API.csproj Kickstart.API/
COPY Kickstart.Application/Kickstart.Application.csproj Kickstart.Application/
COPY Kickstart.Domain/Kickstart.Domain.csproj Kickstart.Domain/
COPY Kickstart.Infrastructure/Kickstart.Infrastructure.csproj Kickstart.Infrastructure/

RUN dotnet restore Kickstart.API/Kickstart.API.csproj

COPY . .
RUN dotnet publish Kickstart.API/Kickstart.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Kickstart.API.dll"]
