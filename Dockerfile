# Use the official .NET 8 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore any dependencies
# It's a good practice to copy the .csproj and restore as a separate layer
# to leverage Docker's build cache.
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application and publish it
# Using -o out to specify the output directory
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the build output from the previous stage (build)
# from the /app/out directory to the current /app directory
COPY --from=build /app/out .

# Install curl for healthchecks or other utilities
# First, update the package lists, then install curl
# The -y flag automatically confirms the installation
RUN apt-get update && \
    apt-get install -y curl && \
    # Clean up the apt cache to reduce image size
    rm -rf /var/lib/apt/lists/*

# Set the entry point for the container
# This command will be executed when the container starts
ENTRYPOINT ["dotnet", "BackEnd.dll"] # Assuming your main DLL is BackEnd.dll

# Expose the port on which the application will run
# This informs Docker that the container listens on this port
EXPOSE 5000
