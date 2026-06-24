
# Developing for Flood Online Reporting Tool - Public

Thank you for your interest in contributing to the Flood Online Reporting Tool - Public! This guide will help you set up your development environment and understand the basic components of the project.

Please ensure you have read our [Code of Conduct](./CODE_OF_CONDUCT.md) and [AI Policy](./AI_POLICY.md) before you start contributing.

## Dependencies

To run Flood Online Reporting Tool - Public with minimal modification, you will need the following:

- **PostgreSQL 13+ with PostGIS extension**: This is the default database provider. The project uses Entity Framework, making it adaptable to other providers with minimal effort.
- **.NET**: Ensure you have the .NET 10 SDK installed.
- **Message system (optional)**: The project includes a messaging system using Azure Service Bus. You can enable or disable this system through the connection string `service-bus` in your configuration.
- **GDS Framework**: The project relies on the Government Digital Service (GDS) framework for its front-end.

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

   - [Install Bun](https://bun.com/docs/installation) and run `bun install` from within the `FloodOnlineReportingTool.Public` project to download the dependencies
   - Previously we recommended using the NPM Task Runner extension in Visual Studio. This can still be used for other tasks, but `install` commands should be run using `bun install` in a separate terminal

4. **AI standards module**:
    
   If you are using GitHub Copilot you can choose to import the Dorset Council UK instruction files. 
   This ensures that code suggestions are more likely to follow the coding standards we are trying to follow at Dorset Council.
   
   To update the standards module, navigate to the `FloodOnlineReportingTool.Public` solution folder and run:
   ```shell
   git submodule update
   ```
   You may need to initialize the submodule the first time you load it after cloning from the source repository. Use the init flag as follows:
   ```shell
   git submodule update --init 
   ```
   Should you need to switch the submodule to a different branch, navigate to the `FloodOnlineReportingTool.Public` solution folder and run:
   ```shell
   git submodule set-branch --branch {branch-name} .github/instructions

   git submodule update --remote .github/instructions
   ```
5. **Set up your secrets**:

   Configure the user secrets file for development. See the "User Secrets and Configuration" section below for details.

6. **Run migrations and seed the database**:

   The database schema and seed data are handled by the data project.
   Navigate to the solution folder and run the following command:
   ```shell
   dotnet ef database update --project "Database" --startup-project "FloodOnlineReportingTool.Public" --context PublicDbContext
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
The project requires a `ConnectionStrings` section in your user secrets. And a connection string named `FloodReportingPublic`. Ensure the connection string contains `;Search Path=fortpublic` for proper functionality.

For an example see `Example secrets file` below.

## User Secrets and Configuration

You must set up user secrets for development. Run the following command in the terminal within the project folder:
- Using Visual Studio:

  Right click the project and select "Manage User Secrets".
- Using .NET CLI:
  ```shell
  dotnet user-secrets init
  ```
Then add your configuration:

- **ConnectionStrings**: Required for database access and optional message system.
- **AzureBlobStorage**: Required for file storage.
  - The `ReadSASToken` is optional and can be used to provide read-only access to the blob storage.
- **GIS**: Used to configure the Address API and other mapping related services.
- **GovNotify**: Used to configure the GovNotify service for sending notifications.
- **Messaging**: Used to configure the messaging system.
- **DownstreamApis**: Used to configure downstream APIs.
- **AzureAd**: Used to configure Azure Active Directory for authentication.

Example secrets file:
```json
{
  "ConnectionStrings": {
    "FloodReportingPublic": "Host=localhost;Port=5432;Database=YourDatabaseName;Username=YourUserName;Password=YourPassword;Search Path=fortpublic",
    "service-bus": "Endpoint=YourEndpoint;SharedAccessKeyName=YourKeyName;SharedAccessKey=YourAccessKey"
  },
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=YourAccountName;AccountKey=YourAccountKey;EndpointSuffix=core.windows.net",
    "ContainerName": "YourContainerName",
    "ReadSASToken": "YourReadSASToken"
  },
  "GIS": {
    "ApiKey": "YourApiKey",
    "AddressSearchUrl": "YourAddressUrl",
    "NearestAddressesUrl": "YourNearestAddressUrl",
    "AccessTokenIssueDurationMonths": 6,
    "OSApiKey": "Your OS Maps API Key here",
    "DataRetentionYears": 7
  },
  "GovNotify": {
    "ApiKey": "YourGovNotifyAPIKey",
    "TestEmail": "test@example.com",
    "Templates": {
      "TestNotification": "TemplateID",
      "VerifyEmailAddress": "TemplateID",
      "ConfirmContactUpdated": "TemplateID",
      "ConfirmContactDeleted": "TemplateID",
      "RequestDpaUpdate": "TemplateID",
      "Unsubscribe": "TemplateID",
      "RequestFullReport": "TemplateID",
      "SendStatusUpdate": "TemplateID",
      "SendPublicComment": "TemplateID",
      "SendCopyOfReport": "TemplateID",
      "RecordDeletion": "TemplateID"
    }
  },
  "Messaging": {
    "Enabled": "true",
    "ConnectionString": "YourServiceBusConnectionString"
  },
  "DownstreamApis": {
    "ReportStatusApi": {
      "BaseUrl": "YourReportStatusApiBaseUrl",
      "Scopes": "ScopesRequired"
    }
  },
  "AzureAd": {
    "ClientId": "YourEntraClientID",
    "ClientSecret": "YourEntraClientSecret",
    "Domain": "YourEntraDomain",
    "Instance": "https://YourEntraInstance/",
    "ResponseType": "code",
    "TenantId": "YourEntraTenantID"
  },
}
```

## Authentication

The project uses Microsoft Identity with OpenID Connect for Authentication.
Authentication is not required to create a flood report. However, certain areas of the application (such as staff/admin views) do require users to be signed in.

## Other Customizations

The project integrates with the Dorset Council Address API. This is customizable via the `GIS` section in your configuration.

For messaging, the connection string in the `Messaging` section allows you to enable or disable the messaging system.

If you want to disable messaging, set `"Enabled": "false"` in your `Messaging` configuration.

## Further Help

For further help, please reach out to the maintainers or consult the documentation. 
