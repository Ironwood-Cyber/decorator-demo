# Decorator Backend Demo with Micro Frontends

## Links

Micro Frontends / Module Federation: https://module-federation.io/guide/start/quick-start.html

JsonForms: https://jsonforms.io/

## Intro

This branch demonstrates a decorator backend architecture with micro frontends. There are 2 backend services (`src/backend/app1-backend/gateway-service` and `src/backend/app3-backend/gateway-service`) and 4 total frontends. Of those frontends, 3 are the producer microfrontends (`src/frontend/app1`, `src/frontend/app2`, and `src/frontend/app3`) while the `src/frontend/host` frontend is the consumer / host of the 3 producer frontends.

Each of the 3 microfrontends contains and exposes a JsonForms component used by the `host` frontend. Of the 3 microfrontends, 2 of them (`app1` and `app3`) have a backend associated with them (`app1-backend/gateway-service` and `app3-backend/gateway-service` respectively).

## Setup and Running

### Docker Compose

This project contains a docker compose file used to run a MongoDB instance (as well as RabbitMQ, however it's not used on this branch) to persist JsonForms data and communicate between the backend services. Before building and running the backends and frontends, run:

```
docker compose up -d mongodb
```

to spin up only the MongoDB container.

If desired, run:

```
docker compose up
```

to spin up both the MongoDB and RabbitMQ containers.

### Backend(s)

#### Step 1, Building

From the root directory of this repo, run:

```
dotnet build
```

#### Step 2, Running the Backends

Navigate into `src/backend/app1-backend/gateway-service` and run:

```
dotnet run
```

From a new terminal window, navigate into `src/backend/app3-backend/gateway-service` and run:

```
dotnet run
```

Both backend services should now be running on port 8080 and 8081 respectively.

In order to modify which decorator DLLs are loaded into the gateway services, please refer to the main branch README section linked here: [README](https://github.com/Ironwood-Cyber/decorator-demo/tree/main?tab=readme-ov-file#modifying-the-gateway-service-configuration-file-to-add-or-remove-decorators).

The `app1-backend` contains a `gateway-service` (the main entrypoint for the app1 API), a `base-service`, and 4 decorator services labeled `decorator-service-1` thru `decorator-service-4`. The `app3-backend` contains a `gateway-service` (the main entrypoint for the app3 API), a `base-service` and 1 decorator labeled `decorator-service-1`.

### Frontend(s)

#### Step 1, Building

To build the frontends, navigate into each frontend application individually (`src/frontend/app1`, `src/frontend/app2`, `src/frontend/app3`, `src/frontend/host`) and run:

```
npm i
```

#### Step 3, Running the Frontends

Each of the 3 apps can be run individually as a standalone frontend, however, for the `host` application to work, all 3 apps need to be running along side the `host` application.

To run an individual frontend, navigate into the appN (where N is 1-3) directory and run:

```
npm run dev
```

When running app1 and/or app3, the respective backend gateway service for the frontend must be run as well for the frontend apps to run correctly.

In order to run the host application, all 3 apps need to be running (as well as the backends for app1 and app3) for the host application to function correctly.

The full steps to run are shown below:

> App1
>
> ```
> # From root dir
> cd src/frontend/app1
> npm run dev
>
> # From root dir
> cd src/backend/app1-backend/gateway-service
> dotnet run
> ```

> App2
>
> ```
> # From root dir
> cd src/frontend/app2
> npm run dev
> ```

> App3
>
> ```
> # From root dir
> cd src/frontend/app3
> npm run dev
>
> # From root dir
> cd src/backend/app3-backend/gateway-service
> dotnet run
> ```

> Host
>
> ```
> # From root dir
> cd src/frontend/host
> npm run dev
> ```
