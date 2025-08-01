using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Media;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Media(
    ILogger<FloodSource> logger,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IJSRuntime JS,
    IGdsJsInterop gdsJs,
    IBlobStorageService blobStorageService
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.Media.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
    ];

    private const int MaxNumFiles = 10;
    private const int MaxFileSizeMB = 20;
    private const long MaxFileSize = MaxFileSizeMB * 1024 * 1024; // Convert MB to bytes

    private static readonly string[] AllowedFileTypes = [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "video/mp4",
        "video/webm",
        "video/ogg",
        "application/pdf",
    ];

    private static readonly string[] FriendlyFileTypes = [
        "JPG", "PNG", "GIF", "MP4", "WEBM", "OGG", "PDF",
    ];

    private readonly CancellationTokenSource _cts = new();
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessageStore = default!;
    private FieldIdentifier _fieldIdentifier;
    private bool _isLoading = true;

    private readonly List<IBrowserFile> _uploadingFiles = [];
    private readonly List<RejectedFile> _rejectedFiles = [];

    private Models.FloodReport.Create.Media Model { get; set; } = default!;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _validationMessageStore = new(_editContext);
        _fieldIdentifier = new(Model, nameof(Model.UploadedFiles));

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isLoading = false;

            // Set any previously entered data
            var data = await GetCreateExtraData();
            if (data is not null && data.Media is not null)
            {
                Model.UploadedFiles = data.Media;
            }

            StateHasChanged();
            
            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnFilesSubmitted(IReadOnlyList<IBrowserFile>? files)
    {
        _isLoading = true;

        if (!ValidateFileUploadLimits(files, out var errorMessage))
        {
            StateHasChanged();
            _isLoading = false;
            return;
        }

        _uploadingFiles.AddRange(files!);
        foreach(var file in files!)
        {
            await ProcessSingleFileAsync(file);
            _uploadingFiles.RemoveAll(f => string.Equals(f?.Name, file.Name, StringComparison.Ordinal));
            StateHasChanged();
        }

        //finished uploading
        CheckValidationStateOfFileUploads();

        _isLoading = false;
        StateHasChanged();
        await UpdateStoredMediaData();
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
            var blobUrl = await blobStorageService.UploadFileToBlobAsync(
                trustedFileNameForFileStorage, 
                file.ContentType, 
                file.OpenReadStream(MaxFileSize, _cts.Token));

            if (blobUrl != null)
            {
                Model.UploadedFiles.Add(new MediaItem
                {
                    Name = file.Name,
                    Url = blobUrl,
                    ContentType = file.ContentType,
                });
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing file {FileName}", file?.Name);
        }
        
        _rejectedFiles.Add(new RejectedFile(file, FileRejectionReason.UploadError));
        return false;
    }

    private async Task DeleteUploadedFile(MediaItem file)
    {
        //confirm
        bool confirmDelete = await JS.InvokeAsync<bool>("confirm", _cts.Token, "Are you sure you want to delete this file?");
        if (confirmDelete)
        {
            //delete from storage
            if (!await blobStorageService.DeleteFileFromBlobByURLAsync(file.Url))
            {
                logger.LogWarning("An error occurred deleting media item with URL {url} from blob storage", file.Url);
            }
            //remove from list
            Model.UploadedFiles.Remove(file);
            CheckValidationStateOfFileUploads();
            StateHasChanged();
            await UpdateStoredMediaData();
        }
    }
    private void DeleteRejectedFile(RejectedFile file)
    {
        // Remove the rejected file from the list
        _rejectedFiles.Remove(file);
        CheckValidationStateOfFileUploads();
        StateHasChanged();
    }

    private bool ValidateFileUploadLimits(IReadOnlyList<IBrowserFile>? files, out string? errorMessage)
    {
        errorMessage = null;

        if (files is null || files.Count > MaxNumFiles)
        {
            errorMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.TooManyFiles, MaxNumFiles);
            _validationMessageStore.Clear();
            _validationMessageStore.Add(_fieldIdentifier, errorMessage);
            _editContext.NotifyValidationStateChanged();
            return false;
        }

        if (files.Count > (MaxNumFiles - Model.UploadedFiles.Count))
        {
            errorMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.TooManyTotalFiles, MaxNumFiles);
            _validationMessageStore.Clear();
            _validationMessageStore.Add(_fieldIdentifier, errorMessage);
            _editContext.NotifyValidationStateChanged();
            return false;
        }

        return true;
    }

    private void CheckValidationStateOfFileUploads()
    {
        // Clear any existing validation errors first
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

    private async Task<ExtraData> GetCreateExtraData()
    {
        var data = await protectedSessionStorage.GetAsync<ExtraData>(SessionConstants.EligibilityCheck_ExtraData);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogDebug("Eligibility Check > Extra Data was not found in the protected storage.");
        return new();
    }

    private async Task UpdateStoredMediaData()
    {
        var createExtraData = await GetCreateExtraData();
        var updatedExtraData = createExtraData with
        {
            Media = Model.UploadedFiles,
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);
    }

    private async Task OnValidSubmit()
    {
        //Media should already be in protected storage at this point
        
        // Go to the next page, which is always the summary
        navigationManager.NavigateTo(FloodReportCreatePages.Summary.Url);
    }
}
