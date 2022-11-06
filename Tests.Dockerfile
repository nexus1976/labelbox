FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /app
COPY labelbox.sln ./
COPY ./labelbox ./labelbox/
COPY ./labelbox.tests ./labelbox.tests/
RUN dotnet restore
RUN dotnet build

ENTRYPOINT ["tail", "-f", "/dev/null"]