using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FloodOnlineReportingTool.Public.Options;
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

    public BlobStorageService(ILogger<BlobStorageService> logger, IOptions<AzureBlobStorageOptions> azureBlobStorageOptions)
    {
        _logger = logger;

        var options = azureBlobStorageOptions.Value;
        _blobStorageConnection = options.ConnectionString;
        _blobContainerName = options.ContainerName;
    }

    private async Task<BlobContainerClient?> GetContainer()
    {
        var container = new BlobContainerClient(_blobStorageConnection, _blobContainerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None).ConfigureAwait(false);
        return container;
    }

    public async Task<bool> DeleteBlobIfExistsAsync(string blobName)
    {
        var container = await GetContainer().ConfigureAwait(false);
        if (container is null)
        {
            return false;
        }

        try
        {
            var deleteResponse = await container
                .DeleteBlobIfExistsAsync(blobName, DeleteSnapshotsOption.IncludeSnapshots)
                .ConfigureAwait(false);

            var existsAndDeleted = deleteResponse.Value;
            if (!existsAndDeleted)
            {
                _logger.LogWarning("Blob {BlobName} does not exist in container {BlobContainerName}. So could not delete it.", blobName, _blobContainerName);
            }

            return existsAndDeleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred deleting blob {BlobName} from container {BlobContainerName}", blobName, _blobContainerName);
            throw;
        }
    }

    public async Task<string> UploadFileToBlobAsync(string fileName, string contentType, Stream fileStream)
    {
        var container = await GetContainer().ConfigureAwait(false);
        if (container is null)
        {
            return "";
        }

        var blobName = fileName.StartsWith("media/", StringComparison.OrdinalIgnoreCase) ? fileName : $"media/{fileName}";

        try
        {
            var blob = container.GetBlobClient(blobName);

            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots).ConfigureAwait(false);
            await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }).ConfigureAwait(false);

            return blob.Name;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred uploading blob {BlobName} from container {BlobContainerName}", blobName, _blobContainerName);
            throw;
        }
    }

    public async Task<(Stream Content, string ContentType)?> OpenReadAsync(string fileName, CancellationToken cancellationToken)
    {
        var container = await GetContainer().ConfigureAwait(false);
        if (container is null)
        {
            return null;
        }

        var blobName = fileName.StartsWith("media/", StringComparison.OrdinalIgnoreCase) ? fileName : $"media/{fileName}";
        var blob = container.GetBlobClient(blobName);
        if (!await blob.ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        var stream = await blob.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return (stream, properties.Value.ContentType);
    }
}