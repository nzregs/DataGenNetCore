FROM microsoft/dotnet:2.1-sdk AS BUILD
WORKDIR /app


# Copy the project and restore packages
COPY *.csproj ./
RUN dotnet restore

# Copy rest and build
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image
# configure settings and keys in env file and pass with docker run
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=BUILD /app/out .
COPY customstart.sh /
ENTRYPOINT ["/app/customstart.sh"]
