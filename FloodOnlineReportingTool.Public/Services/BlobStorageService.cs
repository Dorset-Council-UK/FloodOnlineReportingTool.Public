using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FloodOnlineReportingTool.Public.Settings;
using Microsoft.Extensions.Options;

namespace FloodOnlineReportingTool.Public.Services;

/// <summary>
/// Service to upload and delete files from an Azure Blob Storage container
/// </summary>
/// <remarks>Taken from https://www.c-sharpcorner.com/article/upload-and-delete-files-in-azure-blob-storage-using-blazor-apps-with-net-7/</remarks>
internal class BlobStorageService : IBlobStorageService
{
    private readonly ILogger<BlobStorageService> _logger;
    private readonly string _blobStorageConnection = string.Empty;
    private readonly string _blobContainerName = string.Empty;

    public BlobStorageService(ILogger<BlobStorageService> logger, IOptions<AzureBlobStorageSettings> azureBlobStorageOptions)
    {
        _logger = logger;

        var options = azureBlobStorageOptions.Value;
        _blobStorageConnection = options.ConnectionString;
        _blobContainerName = options.ContainerName;
    }

    private async Task<BlobContainerClient?> GetContainer()
    {
        var container = new BlobContainerClient(_blobStorageConnection, _blobContainerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob).ConfigureAwait(false);
        return container;
    }

    public async Task<bool> DeleteFileFromBlobAsync(string fileName)
    {
        var container = await GetContainer().ConfigureAwait(false);
        if (container is null)
        {
            return false;
        }

        try
        {
            var deleteResponse = await container
                .DeleteBlobIfExistsAsync(fileName, DeleteSnapshotsOption.IncludeSnapshots)
                .ConfigureAwait(false);

            var existsAndDeleted = deleteResponse.Value;
            if (!existsAndDeleted)
            {
                _logger.LogWarning("Blob {FileName} does not exist in container {BlobContainerName}. So could not delete it.", fileName, _blobContainerName);
            }

            return existsAndDeleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred deleting blob {FileName} from container {BlobContainerName}", fileName, _blobContainerName);
            throw;
        }
    }

    public async Task<bool> DeleteFileFromBlobByURLAsync(string url)
    {
        var uri = new Uri(url);
        var fileName = uri.PathAndQuery.Replace($"/{_blobContainerName}/", "", StringComparison.OrdinalIgnoreCase);
        return await DeleteFileFromBlobAsync(fileName).ConfigureAwait(false);
    }

    public async Task<string> UploadFileToBlobAsync(string fileName, string contentType, Stream fileStream)
    {
        var container = await GetContainer().ConfigureAwait(false);
        if (container is null)
        {
            return "";
        }

        var location = fileName.StartsWith("media/", StringComparison.OrdinalIgnoreCase) ? fileName : $"media/{fileName}";

        try
        {
            var blob = container.GetBlobClient(location);

            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots).ConfigureAwait(false);
            await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }).ConfigureAwait(false);

            return blob.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred uploading blob {FileName} from container {BlobContainerName}", location, _blobContainerName);
            throw;
        }
    }
}
