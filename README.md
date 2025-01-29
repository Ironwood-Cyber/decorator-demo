# Setup

Information on how to install all required packages to run the demo.

## Frontend

1. Navigate into the frontend directory:

```bash
cd src/frontend/
```

2. Run the following `npm` command:

```bash
npm install
```

Done!

## Backend

1. From the base project directory run the following `dotnet` command:

```bash
dotnet build
```

Done!

# Running the demo

## Frontend

1. Navigate into the frontend directory:

```bash
cd src/frontend/
```

2. Run the following `npm` command:

```bash
npm run dev
```

The frontend should now be running on local port `3000`.

## Backend

3. In a new terminal session / window, navigate to the backend gateway directory:

```bash
cd src/backend/gateway-service/
```

4. Run the following `dotnet` command:

```bash
dotnet run
```

The backend gateway API should now be running on local port `8080`.

5. Navigate to `http://localhost:3000` to view the running web app.

# Modifying the gateway service configuration file to add or remove decorators

TODO

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "System.Net.Http.HttpClient": "Warning"
    }
  },
  "DllHandlerConfig": {
    "BaseServiceDll": "../base-service/bin/Debug/net8.0/base-service.dll",
    "DecoratorServiceDlls": [
      {
        "ServiceDll": "../decorator-service-1/bin/Debug/net8.0/decorator-service-1.dll",
        "ExecutionOrder": "None"
      },
      {
        "ServiceDll": "../decorator-service-2/bin/Debug/net8.0/decorator-service-2.dll",
        "ExecutionOrder": "None"
      },
      {
        "ServiceDll": "../decorator-service-3/bin/Debug/net8.0/decorator-service-3.dll",
        "ExecutionOrder": "OverrideBaseService"
      },
      {
        "ServiceDll": "../decorator-service-4/bin/Debug/net8.0/decorator-service-4.dll",
        "ExecutionOrder": "OverrideBaseService"
      }
    ]
  }
}
```

# Roadmap

1. Dockerize
