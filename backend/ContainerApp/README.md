# Backend.sln

# Distributed Task Management System

This project is a distributed microservices-based system for managing tasks. It leverages [Dapr](https://dapr.io/) for service discovery, inter-service communication, and message-based workflows. PostgreSQL is used as the main data store. All services are containerized and orchestrated using Docker Compose.

---

## 🧱 Architecture Overview

### Core Services

- **Manager Service**
  - Orchestrates task lifecycle: creation, updates, and deletion.
  - Initiates task execution and publishes relevant events.

- **Engine Service**
  - Responsible for executing or processing tasks.
  - Subscribes to task-related events via pub/sub topics.

- **Accessor Service**
  - Provides CRUD operations on the `Task` table.
  - Communicates directly with PostgreSQL.

---





## 🛠️ Technology Stack

- **.NET 9** with **Minimal APIs**
- **Azure Service Bus Emulator** for local messaging (topics and queues)
- **Dapr** for service discovery, pub/sub abstraction, and service invocation
- **Docker & Docker Compose** for container orchestration
- **PostgreSQL** for data persistence
- **pgAdmin** for DB inspection and query execution

---



### Supporting Components

- **Dapr Sidecars**
  - Each service is attached to a Dapr sidecar for service invocation and message communication.
  - Enables service discovery, bindings, and pub/sub without tight coupling.

- **Service Bus Emulator**
  - Emulates a distributed message broker.
  - **Pub/Sub Topics**: Used for event-driven communication between services.
  - **Bindings**: Used to simulate **queues** for point-to-point message delivery.

- **PostgreSQL Database**
  - Stores structured task data in a centralized `Task` table.

- **pgAdmin**
  - Web-based UI to view and query PostgreSQL data.
  - Useful for development and debugging.

---




## 🧩 Data Model

### `Task` Table (PostgreSQL)

| Column Name | Type  | Description        |
|-------------|-------|--------------------|
| Id          | INT   | Primary key        |
| Name        | TEXT  | Task title         |
| Payload     | TEXT  | Task description   |

---




## 🗃️ Accessing PostgreSQL via pgAdmin



- **pgAdmin URL**: [http://localhost:8080](http://localhost:8080)
- **Default Credentials**:
  - **Email**: `admin@admin.com`
  - **Password**: `admin`

### Register the Database Server in pgAdmin

1. Open `http://localhost:8080` and log in.
2. Right-click on **Servers** > **Register** > **Server...**
3. In the **General** tab:
   - **Name**: `Postgres DB` (or any custom name)
4. In the **Connection** tab:
   - **Host name/address**: `postgres-db`
   - **Port**: `5432`
   - **Maintenance database**: `postgres_db`
   - **Username**: `postgres`
   - **Password**: `postgres`
   - ✅ Check **Save Password**
5. Click **Save**.

### Viewing the Task Table

1. Navigate to:  
   `Servers` > `Postgres DB` > `Databases` > `postgres_db` > `Schemas` > `public` > `Tables` > `Task`
2. Right-click the `Task` table and choose  
   **View/Edit Data** > **All Rows**

---




## 🔄 Inter-Service Communication



### Dapr Service Invocation

- Services communicate with each other using Dapr's HTTP or gRPC APIs.
- Example: `Manager` invokes `Accessor` to get a task.

 
### Dapr Pub/Sub (Topics)

- Used for asynchronous question/answer exchange between the Manager and AI services.
- Example: `Manager` publishes questions to the manager-to-ai topic and receives answers via the ai-to-manager topic, enabling decoupled communication with the AI processor.

 
### Dapr Bindings (Queues)

- Used to simulate **queue-based** communication via output/input bindings.
- Services emit messages to queues using output bindings and consume them via input bindings.
- Example: `Engine` posts a task result to a queue; `Accessor` reads it to update DB.

---




## ▶️ Running the System

### Prerequisites

Ensure the following are installed:

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- [Dapr CLI](https://docs.dapr.io/get-dapr/cli/)

---

### Start the System

```bash
cd containerApp

docker-compose up --build
```