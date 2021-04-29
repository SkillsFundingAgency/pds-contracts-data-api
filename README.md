# Manage your education and skills funding - Contracts Data API

## Introduction

Contracts Data API provides a service to administer contracts, content and documents in "Manage your education and skills funding". This is a BETA version and is currently used only by a limited number of components.

### Getting Started

This is a self-contained Visual Studio 2019 solution containing a number of projects (web application, service and repository layers, with associated unit test and integration test projects).
To run this product locally, you will need to configure the list of dependencies, once configured and the configuration files updated, it should be F5 to run and debug locally.

### Installing

Clone the project and open the solution in Visual Studio 2019.

#### List of dependencies

| Item | Purpose |
|-------|-------|
| SQL database | Contract maintenance |
| Audit Api | Audit API provides a single shared pacakge to audit events |
| Azure Storage Emulator | The Microsoft Azure Storage Emulator is a tool that emulates the Azure Blob, Queue, and Table services for local development purposes.|
| Azure Storage Explorer | To maintain local blobs and queues for local development purposes. |

#### SQL (Azure) Database

You can choose any of the following for local development
* Microsoft SQL Server Express
* Microsoft SQL Server
* SQL Azure

In reality we would recomend using MS SQL express for local development.

#### Audit API

Audit API can be found at <https://github.com/SkillsFundingAgency/pds-shared-audit-api>.

#### Azure Storage Emulator/Azure Storage Explorer

The Storage Emulator/Azure Storage Explorer is available as part of the Microsoft Azure SDK. Azure blob storage requires it for local development.

Further information can be found at:
* <https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator>
* <https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage>
* <https://docs.microsoft.com/en-us/dotnet/api/azure.storage.blobs?view=azure-dotnet>

### Local Config Files

Once you have cloned the public repo you need the following configuration files listed below.

| Location | config file |
|-------|-------|
| Pds.Contracts.Data.Api | appsettings.development.json |

The following is a sample configuration file

```json
{
    "ConnectionStrings": {
        "contracts": "Server=replace_server;Database=replace_dbname;Trusted_Connection=True;"
    },
    "Authentication": {
        "Instance": "replace_azure_active_directory_instance",
        "TenantId": "replace_azure_active_directory_tenantId",
        "ClientId": "replace_azure_active_directory_clientid"
    },
	"AuditApiConfiguration": {
		"ApiBaseAddress": "replace_azure_environment",
		"ShouldSkipAuthentication": true
	},
	"AzureBlobConfiguration": {
		"ConnectionString": "replace_azure_storage_connectionstring",
		"ContainerName": "replace_azure_storage_containername"
	},
	"HttpPolicyOptions": {
		"HttpRetryCount": 3,
		"HttpRetryBackoffPower": 2,
		"CircuitBreakerToleranceCount": 5,
		"CircuitBreakerDurationOfBreak": "0.00:00:30"
	},
    "AllowedHosts": "*",
    "Logging": {
        "LogLevel": {
            "Default": "Information"
        }
    }
}
```

The following configurations needs to be replaced with your values.

| Key | Token | Example |
|-|-|-|
| ConnectionStrings.server | replace_server | (local) |
| ConnectionStrings.contracts | replace_dbname | ContractsDb |
| Authentication.Instance | replace_azure_active_directory_instance | <https://localhost/> |
| Authentication.TenantId | replace_azure_active_directory_tenantId | empty_string |
| Authentication.ClientId | replace_azure_active_directory_clientid | empty_string |
| AuditApiConfiguration.ApiBaseAddress | replace_azure_environment | <https://sample.azurewebsites.net> |
| AzureBlobConfiguration.ConnectionString | replace_azure_storage_connectionstring | DefaultEndpointsProtocol=https;AccountName={containername};AccountKey={accountkey};EndpointSuffix=core.windows.net |
| AzureBlobConfiguration.ContainerName | replace_azure_storage_containername | yourcontainername |

## API references

Install and configure project to run locally, F5 to start the instance which should by default display Open API documentation, it can be found at <https://localhost:44389/swagger>.

## Build and Test

This API is built using

* Microsoft Visual Studio 2019
* .Net Core 3.1

To build and test locally, you can either use visual studio 2019 or VSCode or simply use dotnet CLI `dotnet build` and `dotnet test` more information in dotnet CLI can be found at <https://docs.microsoft.com/en-us/dotnet/core/tools/>.

## Configuring for release

In addition to development configuration, this API requires additional configuration for release environment, the following provides a sample config that you will need to configure.

```json
{
    "ConnectionStrings": {
        "contracts": "Server=replace_server;Database=replace_dbname;Trusted_Connection=True;"
    },
    "Authentication": {
        "Instance": "replace_azure_active_directory_instance",
        "TenantId": "replace_azure_active_directory_tenantId",
        "ClientId": "replace_azure_active_directory_clientid"
    },
	"AuditApiConfiguration": {
		"ApiBaseAddress": "replace_azure_environment",
		"Authority": "replace_azure_active_directory_authority",
		"ClientId": "replace_azure_active_directory_clientid",
		"ClientSecret": "replace_azure_active_directory_clientsecret",
		"TenantId": "replace_azure_active_directory_tenantId",
		"AppUri": "replace_azure_active_directory_appuri"
	},
	"AzureBlobConfiguration": {
		"ConnectionString": "replace_azure_storage_connectionstring",
		"ContainerName": "replace_azure_storage_containername"
	},
	"HttpPolicyOptions": {
		"HttpRetryCount": 3,
		"HttpRetryBackoffPower": 2,
		"CircuitBreakerToleranceCount": 5,
		"CircuitBreakerDurationOfBreak": "0.00:00:30"
	},
    "AllowedHosts": "*",
    "PdsApplicationInsights": {
        "InstrumentationKey": "replace_application_insights_instrumentationkey",
        "Environment": "replace_environment"
    },
    "Logging": {
        "ApplicationInsights": {
            "LogLevel": {
                "Default": "Information",
                "Microsoft": "Error"
            }
        },
        "LogLevel": {
            "Default": "Information"
        }
    }
}
```

The following configurations needs to be replaced with your values.

|Key|Token|Example|
|-|-|-|
|ConnectionStrings.contracts|replace_server|SQL Server or SQl Azure server instance|
|ConnectionStrings.contracts|replace_dbname|ContractsDb|
|Authentication.Instance|replace_azure_active_directory_instance|<https://login.microsoftonline.com/>|
|Authentication.TenantId|replace_azure_active_directory_tenantid|Valid azure active directory tenant id|
|Authentication.ClientId|replace_azure_active_directory_clientid|Valid application id in azure active directory|
|PdsApplicationInsights.InstrumentationKey|replace_application_insights_instrumentationkey|Valid azure application instance instrumentation id|
|PdsApplicationInsights.Environment|replace_environment|Any string to identify your environment e.g. LocalDevelopment|
| AuditApiConfiguration.ApiBaseAddress | replace_azure_environment | <https://sample.azurewebsites.net> |
| AzureBlobConfiguration.ConnectionString | replace_azure_storage_connectionstring | DefaultEndpointsProtocol=https;AccountName={containername};AccountKey={accountkey};EndpointSuffix=core.windows.net |
| AzureBlobConfiguration.ContainerName | replace_azure_storage_containername | yourcontainername |
| AuditApiConfiguration.ApiBaseAddress | replace_azure_environment | <https://sample.azurewebsites.net> |
| AuditApiConfiguration.Authority | replace_azure_active_directory_authority | <https://login.microsoftonline.com/> |
| AuditApiConfiguration.TenantId | replace_azure_active_directory_tenantid | Valid azure active directory tenant id |
| AuditApiConfiguration.ClientId | replace_azure_active_directory_clientid | Valid application id in azure active directory |
| AuditApiConfiguration.ClientSecret | replace_azure_active_directory_clientsecret | Valid application secret in azure active directory |
| AuditApiConfiguration.AppUri | replace_azure_active_directory_appuri | Valid scope in azure active directory |

## Contribute

To contribute,

* If you are part of the team then create a branch for changes and then then submit your changes for review by creating a pull request.
* If you are external to the organisation then fork this repository and make necessary changes and then submit your changes for review by creating a pull request.
