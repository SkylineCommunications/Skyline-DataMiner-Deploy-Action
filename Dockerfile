
# Set the base image as the .NET 6.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
WORKDIR /app
COPY . ./
RUN dotnet publish ./GitHubAction/GitHubAction.Console/GitHubAction.Console.csproj -c Release -o out --no-self-contained

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY --from=build-env /app/out .
COPY --from=build-env /app/InstallScript ./InstallScript
RUN apt-get -y update
RUN apt-get -y install git
ENTRYPOINT [ "dotnet", "/GitHubAction.Console.dll" ]