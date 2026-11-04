using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Media;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Media;

[Authorize]
public partial class Index(
    ILogger<Index> logger,
    NavigationManager navigationManager,
    IJSRuntime JS,
    IBlobStorageService blobStorageService,
    IMediaItemRepository mediaItemRepository
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = MediaPages.Home.Title;

    [Parameter]
    public Guid FloodReportSourceId { get; set; }
    [SupplyParameterFromQuery(Name = "back_url")]
    public string BackURL { get; set; } = "confirmation";

    private PageInfo PreviousPage => string.Equals(BackURL, "confirmation", StringComparison.OrdinalIgnoreCase) ? 
        FloodReportCreatePages.Confirmation : 
        FloodReportPages.Overview;

    private const int MaxNumFiles = 10;
    private const int MaxFileSizeMB = 20;
    private const long MaxFileSize = MaxFileSizeMB * 1024 * 1024; // Convert MB to bytes
    private const int ThumbnailWidthPixels = 150;
    private const int ThumbnailHeightPixels = 100;
    private const int ThumbnailJpegQuality = 75;
    private const string ThumbnailContentType = "image/jpeg";
    private const string ThumbnailSuffix = ".thumbnail.jpg";

    private int RemainingSlots => MaxNumFiles - Model.UploadedFiles.Count;
    private static readonly string[] ImageFileTypes = [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
    ];
    private static readonly string[] VideoFileTypes = [
        "video/mp4",
        "video/webm",
        "video/ogg",
    ];
    private static readonly string[] DocumentFileTypes = [
        "application/pdf",
    ];
    private static readonly string[] AllowedFileTypes = [
        .. ImageFileTypes, 
        .. VideoFileTypes, 
        .. DocumentFileTypes,
    ];

    private static readonly string[] FriendlyFileTypes = [
        "JPG", "PNG", "GIF", "MP4", "WEBM", "OGG", "PDF",
    ];

    private readonly CancellationTokenSource _cts = new();
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessageStore = default!;
    private FieldIdentifier _fieldIdentifier;

    private bool _isSubmittingFiles = false;

    private readonly List<IBrowserFile> _uploadingFiles = [];
    private readonly List<RejectedFile> _rejectedFiles = [];
    private string? _uploadLimitError;

    private Models.FloodReport.Create.MediaItem? _renamingFile;
    private string _renameText = string.Empty;
    private string? _renameError;

    private Models.FloodReport.Create.Media Model { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _validationMessageStore = new(_editContext);
        _fieldIdentifier = new(Model, nameof(Model.UploadedFiles));

        if (FloodReportSourceId == Guid.Empty)
        {
            logger.LogWarning("Flood report source ID was not available");
            _validationMessageStore.Add(_fieldIdentifier, "You are not able to add media without a related flood report source");
            _editContext.NotifyValidationStateChanged();
        }
        else
        {
            await LoadExistingMediaItems();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error occurred during disposal");
        }

        GC.SuppressFinalize(this);
    }

    private async Task LoadExistingMediaItems()
    {
        var mediaItems = await mediaItemRepository.GetByReport(FloodReportSourceId, _cts.Token);
        Model.UploadedFiles = [.. mediaItems
            .Select(mediaItem => new Models.FloodReport.Create.MediaItem
            {
                Id = mediaItem.Id,
                Name = mediaItem.Title ?? Path.GetFileName(mediaItem.BlobName),
                BlobName = mediaItem.BlobName,
                ContentType = mediaItem.ContentType,
            })];
    }

    private async Task OnFilesSubmitted(IReadOnlyList<IBrowserFile>? files)
    {
        _isSubmittingFiles = true;

        if (!ValidateFileUploadLimits(files))
        {
            StateHasChanged();
            _isSubmittingFiles = false;
            return;
        }

        _uploadingFiles.AddRange(files!);
        foreach (var file in files!)
        {
            await ProcessSingleFileAsync(file);
            _uploadingFiles.RemoveAll(f => string.Equals(f?.Name, file.Name, StringComparison.Ordinal));
            StateHasChanged();
        }

        //finished uploading
        CheckValidationStateOfFileUploads();

        _isSubmittingFiles = false;
        StateHasChanged();
    }

    private async Task<bool> ProcessSingleFileAsync(IBrowserFile file)
    {
        if (!AllowedFileTypes.Contains(file.ContentType))
        {
            _rejectedFiles.Add(new RejectedFile(file, FileRejectionReason.InvalidFileType));
            return false;
        }

        if (file.Size > MaxFileSize)
        {
            _rejectedFiles.Add(new RejectedFile(file, FileRejectionReason.FileTooLarge));
            return false;
        }

        try
        {
            var trustedFileNameForFileStorage = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
            await using var fileStream = file.OpenReadStream(MaxFileSize, _cts.Token);
            var blobName = await blobStorageService.UploadFileToBlobAsync(
                trustedFileNameForFileStorage,
                file.ContentType,
                fileStream);

            if (!string.IsNullOrWhiteSpace(blobName))
            {
                if (IsImageFileType(file.ContentType))
                {
                    try
                    {
                        await UploadThumbnailAsync(file, trustedFileNameForFileStorage);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "An error occurred while creating thumbnail for file {FileName}", file.Name);
                    }
                }

                Model.UploadedFiles.Add(new Models.FloodReport.Create.MediaItem
                {
                    Name = Path.GetFileNameWithoutExtension(file.Name),
                    BlobName = blobName,
                    ContentType = file.ContentType,
                });
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing file {FileName}", file.Name);
        }

        _rejectedFiles.Add(new RejectedFile(file, FileRejectionReason.UploadError));
        return false;
    }

    private static bool IsImageFileType(string contentType)
    {
        return ImageFileTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsVideoFileType(string contentType)
    {
        return VideoFileTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsDocumentFileType(string contentType)
    {
        return DocumentFileTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetThumbnailFromBlobName(string blobName)
    {
        return $"{blobName}{ThumbnailSuffix}";
    }

    private string GetMediaItemUrl(Models.FloodReport.Create.MediaItem mediaItem)
    {
        var encodedBlobName = Uri.EscapeDataString(mediaItem.BlobName);
        return $"api/media/{FloodReportSourceId}?blobName={encodedBlobName}";
    }
    private string GetMediaItemThumbnailUrl(Models.FloodReport.Create.MediaItem mediaItem)
    {
        var encodedBlobName = Uri.EscapeDataString(GetThumbnailFromBlobName(mediaItem.BlobName));
        return $"api/media/{FloodReportSourceId}?blobName={encodedBlobName}";
    }

    private async Task UploadThumbnailAsync(IBrowserFile file, string trustedFileNameForFileStorage)
    {
        await using var sourceStream = file.OpenReadStream(MaxFileSize, _cts.Token);
        using var image = await Image.LoadAsync(sourceStream, _cts.Token);

        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(ThumbnailWidthPixels, ThumbnailHeightPixels),
        }));

        await using var thumbnailStream = new MemoryStream();
        await image.SaveAsJpegAsync(
            thumbnailStream,
            new JpegEncoder { Quality = ThumbnailJpegQuality },
            _cts.Token);

        thumbnailStream.Position = 0;

        var thumbnailFileName = $"{trustedFileNameForFileStorage}{ThumbnailSuffix}";
        await blobStorageService.UploadFileToBlobAsync(thumbnailFileName, ThumbnailContentType, thumbnailStream);
    }

    private void StartRename(Models.FloodReport.Create.MediaItem file)
    {
        _renamingFile = file;
        _renameText = file.Name;
        _renameError = null;
    }

    private void CancelRename()
    {
        _renamingFile = null;
        _renameText = string.Empty;
        _renameError = null;
    }

    private const int MaxRenameTitleLength = 255;

    private async Task SaveRename(Models.FloodReport.Create.MediaItem file)
    {
        _renameError = null;

        if (string.IsNullOrWhiteSpace(_renameText))
        {
            _renameError = "Enter a file name";
            return;
        }

        if (_renameText.Length > MaxRenameTitleLength)
        {
            _renameError = $"File name must be {MaxRenameTitleLength} characters or fewer";
            return;
        }

        if (file.Id.HasValue)
        {
            var result = await mediaItemRepository.UpdateTitle(file.Id.Value, _renameText, _cts.Token);
            if (!result.IsSuccess)
            {
                _renameError = string.Join(", ", result.Errors);
                return;
            }
        }

        file.Name = _renameText;
        _renamingFile = null;
        _renameText = string.Empty;
    }

    private async Task DeleteUploadedFile(Models.FloodReport.Create.MediaItem file)
    {
        //confirm
        bool confirmDelete = await JS.InvokeAsync<bool>("confirm", _cts.Token, "Are you sure you want to delete this file?");
        if (confirmDelete)
        {
            if (file.Id.HasValue)
            {
                var deleteMediaItem = await mediaItemRepository.Delete(file.Id.Value, _cts.Token);
                if (!deleteMediaItem.IsSuccess)
                {
                    foreach (var error in deleteMediaItem.Errors)
                    {
                        logger.LogWarning("Couldn't delete a media item: {ErrorMessage}", error);
                    }

                    _validationMessageStore.Add(_fieldIdentifier, "Sorry, something went wrong");
                    _editContext.NotifyValidationStateChanged();
                    return;
                }
            }

            //delete from storage
            if (!await blobStorageService.DeleteBlobIfExistsAsync(file.BlobName))
            {
                logger.LogWarning("An error occurred deleting media item with blob name {BlobName} from blob storage", file.BlobName);
            }

            if (IsImageFileType(file.ContentType))
            {
                var thumbnailBlobName = GetThumbnailFromBlobName(file.BlobName);
                if (!await blobStorageService.DeleteBlobIfExistsAsync(thumbnailBlobName))
                {
                    logger.LogWarning("An error occurred deleting media thumbnail with blob name {BlobName} from blob storage", thumbnailBlobName);
                }
            }

            //remove from list
            Model.UploadedFiles.Remove(file);
            CheckValidationStateOfFileUploads();
            StateHasChanged();
        }
    }
    private void DeleteRejectedFile(RejectedFile file)
    {
        // Remove the rejected file from the list
        _rejectedFiles.Remove(file);
        CheckValidationStateOfFileUploads();
        StateHasChanged();
    }

    private bool ValidateFileUploadLimits(IReadOnlyList<IBrowserFile>? files)
    {
        _uploadLimitError = null;

        if (files is null || files.Count > MaxNumFiles)
        {
            _uploadLimitError = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.TooManyFiles, MaxNumFiles);
            return false;
        }

        if (files.Count > (MaxNumFiles - Model.UploadedFiles.Count))
        {
            _uploadLimitError = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.TooManyTotalFiles, MaxNumFiles);
            return false;
        }

        return true;
    }

    private void CheckValidationStateOfFileUploads()
    {
        // Clear any existing validation errors first
        _uploadLimitError = null;
        _validationMessageStore.Clear();

        // Add validation errors for specific fields
        if (_rejectedFiles.Count > 0)
        {
            foreach (var rejectedFile in _rejectedFiles)
            {

                _validationMessageStore.Add(_fieldIdentifier, GetErrorMessageForFile(rejectedFile));
            }
        }

        // Notify the EditContext that validation state has changed
        _editContext.NotifyValidationStateChanged();
    }

    private static string GetErrorMessageForFile(RejectedFile rejectedFile)
    {
        var errorMessage = ErrorMessageConstants.UploadError;
        switch (rejectedFile.FileRejectionReason)
        {
            case FileRejectionReason.InvalidFileType:
                errorMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.InvalidFileType, FriendlyFileTypes.JoinWithOr());
                break;
            case FileRejectionReason.FileTooLarge:
                errorMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.FileTooLarge, MaxFileSizeMB);
                break;
        }
        return errorMessage;
    }

    private async Task OnValidSubmit()
    {
        if (FloodReportSourceId == Guid.Empty)
        {
            return;
        }

        _validationMessageStore.Clear();
        _editContext.NotifyValidationStateChanged();

        if (Model.UploadedFiles.Count == 0)
        {
            navigationManager.NavigateTo(PreviousPage.Url);
            return;
        }

        var mediaItems = Model.UploadedFiles
            .Where(file => !file.Id.HasValue)
            .Select(file => new Database.Models.MediaItem
            {
                UploadDateUtc = DateTimeOffset.UtcNow,
                Title = file.Name,
                BlobName = file.BlobName,
                ContentType = file.ContentType,
            })
            .ToList();

        if (mediaItems.Count == 0)
        {
            navigationManager.NavigateTo(PreviousPage.Url);
            return;
        }

        var createMediaItems = await mediaItemRepository.Create(FloodReportSourceId, mediaItems, _cts.Token);
        if (!createMediaItems.IsSuccess)
        {
            foreach (var error in createMediaItems.Errors)
            {
                logger.LogWarning("Couldn't create a media item: {ErrorMessage}", error);
            }

            _validationMessageStore.Add(_fieldIdentifier, "Sorry, something went wrong");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        navigationManager.NavigateTo(PreviousPage.Url);
    }
}
