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

Below is the gateway service configuration file found at `src/backend/gateway-service/appsettings.json`. Under the `DllHandlerConfig.DecoratorServiceDlls` field, each decorator library DLL is listed along with an execution order on how to process the event handling logic. Some decorator libraries do not have event handling logic, which is why some are marked as `None`. The below config contains a list entry for all the decorator libraries, but the entries can be removed / added back to the config during runtime to modify the data presented on the frontend:

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

For example, the configuration below will only use the base library and the decorator 1 library:

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
        "ServiceDll": "../decorator-service-2/bin/Debug/net8.0/decorator-service-2.dll",
        "ExecutionOrder": "None"
      }
    ]
  }
}
```

The `ServiceDll` field is a relative path to the specific DLL relative to the `src/backend/gateway-service` directory.

# Roadmap

1. Dockerize
