FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src
COPY labelbox.sln ./
COPY labelbox/*.csproj ./labelbox/
COPY labelbox.tests/*.csproj ./labelbox.tests/
RUN dotnet restore
COPY . .

WORKDIR /src/labelbox
RUN dotnet build -c Release -o /app

FROM build-env AS publish
RUN dotnet publish "labelbox.csproj" -c Release -o /app/publish

FROM base as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "labelbox.dll"]