namespace FloodOnlineReportingTool.Public.Services
{
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads a file to the configured Azure Blob Storage container
        /// </summary>
        /// <returns>The blob name</returns>
        Task<string> UploadFileToBlobAsync(string fileName, string contentType, Stream fileStream);

        /// <summary>
        /// Deletes a blob from the configured Azure Blob Storage container if it exists
        /// </summary>
        /// <returns>True if the blob existed and was deleted, false otherwise</returns>
        Task<bool> DeleteBlobIfExistsAsync(string blobName);

        /// <summary>
        /// Opens a stream for reading from the blob and configured Azure Blob Storage container
        /// </summary>
        /// <returns>A tuple containing the stream and the content type, or null if the blob does not exist</returns>
        Task<(Stream Content, string ContentType)?> OpenReadAsync(string fileName, CancellationToken cancellationToken);
    }
}