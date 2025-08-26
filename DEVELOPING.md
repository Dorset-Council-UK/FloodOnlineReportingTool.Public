
# Developing for Flood Online Reporting Tool - Public

Thank you for your interest in contributing to the Flood Online Reporting Tool - Public! This guide will help you set up your development environment and understand the basic components of the project.

## Dependencies

To run Flood Online Reporting Tool - Public with minimal modification, you will need the following:

- **PostgreSQL 13+ with PostGIS extension**: This is the default database provider. The project uses Entity Framework, making it adaptable to other providers with minimal effort.
- **.NET 9**: Ensure you have the .NET 9 SDK installed.
- **Message system (optional)**: The project includes a messaging system using RabbitMQ. You can enable or disable this system through the `RabbitMQ:Enabled` setting in your configuration.
- **GDS Framework**: The project relies on the Government Digital Service (GDS) framework for its front-end. Make sure to run `npm install` to set up dependencies.

## Getting Started

1. **Clone the repository**:
   ```shell
   git clone <repository-url>
   cd <repository-folder>
   ```

2. **Create a new branch**:
   ```shell
   git checkout -b <branch-name>
   ```

3. **Install front-end dependencies**:

   Run the following command in the root folder:
   ```shell
   npm install
   ```

4. **Set up your secrets**:

   Configure the user secrets file for development. See the "User Secrets and Configuration" section below for details.

5. **Run migrations and seed the database**:

   The database schema and seed data are handled by the data project.
   Navigate to the solution folder and run the following command:
   ```shell
   dotnet ef database update --project "Database\Database.csproj" --startup-project "FloodOnlineReportingTool.Public\FloodOnlineReportingTool.Public.csproj" --context PublicDbContext
   ```
6. **Run migrations for the user database**:

   Navigate to the solution folder and run the following command:
   ```shell
   dotnet ef database update --project "Database\Database.csproj" --startup-project "FloodOnlineReportingTool.Public\FloodOnlineReportingTool.Public.csproj" --context UserDbContext
   ```

## Database Setup

Flood Online Reporting Tool - Public uses PostgreSQL as its default database. Migrations and data seeding are automated, so no manual database setup is required.

### User
Create a default user with the following credentials:
- **Username**: `fort`
- **Password**: Any password you choose
- **Permissions**: can log in, can create databases

The username and password is then used in the connection string FloodReportingPublic.

### Connection Strings
The project requires a `ConnectionStrings` section in your user secrets. And a connection string named `FloodReportingPublic`. Ensure the connection string contains `;Search Path=fortpublic` for proper functionality. See `Example secrets file` below for a more an example.

## User Secrets and Configuration

You must set up user secrets for development. Run the following command in the terminal within the project folder:
- Using Visual Studio:

  Right click the project and select "Manage User Secrets".
- Using .NET CLI:
  ```shell
  dotnet user-secrets init
  ```
Then add your configuration:

- **GIS**: Used to configure the Dorset Council Address API (customizable).
- **RabbitMQ**: Used for the optional messaging system.
- **ConnectionStrings**: Required for database access.

Example secrets file:
```json
{
  "ConnectionStrings": {
    "FloodReportingPublic": "Host=localhost;Port=5432;Database=YourDatabaseName;Username=YourUserName;Password=YourPassword;SearchPath=fortpublic"
  },
  "GIS": {
    "ApiKey": "YourApiKey",
    "AddressSearchUrl": "YourAddressUrl",
    "NearestAddressesUrl": "YourNearestAddressUrl",
    "AccessTokenIssueDurationMonths": 6,
    "OSApiKey": "Your OS Maps API Key here"
  },
  "RabbitMQ": {
    "Enabled": "false",
    "Host": "amqp://localhost:5672",
    "HostContainer": "amqp://fort-queue:5672",
    "Username": "YourMessagingUserName",
    "Password": "YourMessagingPassword"
  }
}
```

## Authentication

Authentication is required after creating a flood report. The project uses standard .NET cookie authentication for handling user sessions. Some parts of the application, such as form creation, are accessible without authentication.

## Other Customizations

The project integrates with the Dorset Council Address API. This is customizable via the `GIS` section in your configuration.

For messaging, the `RabbitMQ` section allows you to enable or disable the MassTransit messaging system. By default, messaging is disabled (`RabbitMQ:Enabled = false`).

## Further Help

For further help, please reach out to the maintainers or consult the documentation. 
