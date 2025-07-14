using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Media(
    ILogger<FloodSource> logger,
    ICommonRepository commonRepository,
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

    private Models.FloodReport.Create.Media Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<IBrowserFile?> uploadingFiles = [];
    //private IList<MediaItem> uploadedFiles = [];
    private IList<RejectedFile> rejectedFiles = [];
    private const int MaxFileSizeMB = 20;
    private const long MaxFileSize = MaxFileSizeMB * 1024 * 1024; // Convert MB to bytes
    private string[] AllowedFileTypes { get; } = [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "video/mp4",
        "video/webm",
        "video/ogg",
        "application/pdf",
    ];

    private string errorMessage = string.Empty;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception)
        {
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

    private async void OnValidFilesSubmitted(GdsBlazorComponents.FileInputResult result)
    {
        _isLoading = true;
        //warn user about rejected files
        if (result.RejectedFiles.Any())
        {
            //show rejected files in list
            rejectedFiles = [.. rejectedFiles, .. result.RejectedFiles];
        }
        // Upload valid files to blob storage
        uploadingFiles = [.. result.AcceptedFiles];
        foreach (var file in result.AcceptedFiles)
        {
            try
            {
                var trustedFileNameForFileStorage = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(file.Name)}";
                var originalFileName = file.Name;
                var blobUrl = await blobStorageService.UploadFileToBlobAsync(trustedFileNameForFileStorage, file.ContentType, file.OpenReadStream(MaxFileSize));

                if (blobUrl != null)
                {
                    //remove file name from uploadingFiles if it was already there
                    uploadingFiles = [.. uploadingFiles.Where(f => !string.Equals(f?.Name, originalFileName, StringComparison.Ordinal))];
                    var newMediaItem = new MediaItem
                    {
                        Name = originalFileName,
                        Url = blobUrl,
                        ContentType = file.ContentType,
                    };
                    // Add the uploaded file URL to the model
                    Model.UploadedFiles.Add(newMediaItem);
                    StateHasChanged();
                }
                else
                {
                    rejectedFiles = [.. rejectedFiles, new RejectedFile(file,FileRejectionReason.UploadError)];
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                // Handle file input errors
                logger.LogError(ex, "An error occurred while processing file {FileName}.", file?.Name);
                rejectedFiles = [.. rejectedFiles, new RejectedFile(file, FileRejectionReason.UploadError)];

            }
        }
        
        _isLoading = false;
        StateHasChanged();
        await UpdateStoredMediaData();
    }

    private async Task DeleteUploadedFile(MediaItem file)
    {
        //confirm
        bool ConfirmDelete = await JS.InvokeAsync<bool>("confirm", "Are you sure you want to delete this file?");
        if (ConfirmDelete)
        {
            //delete from storage
            if (!await blobStorageService.DeleteFileFromBlobByURLAsync(file.Url))
            {
                logger.LogWarning("An error occurred deleting media item with URL {0} from blob storage", file.Url);
            }
            //remove from list
            Model.UploadedFiles = [.. Model.UploadedFiles.Where(f => f != file)];
            StateHasChanged();
            await UpdateStoredMediaData();
        }
    }

    private void DeleteRejectedFile(RejectedFile file)
    {
        // Remove the rejected file from the list
        rejectedFiles = [.. rejectedFiles.Where(f => f != file)];
        StateHasChanged();
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

    private async void OnFileInputError(Exception ex)
    {
        if(ex is TooManyFilesSelectedException fileInputException)
        {
            // Handle specific file input errors
            errorMessage = $"You can only select up to {fileInputException.MaxAllowedFiles} files at the same time.";
        }
        else
        {
            // Handle general errors
            logger.LogError(ex, "An error occurred while processing files.");
        }
    }

    private async Task OnValidSubmit()
    {
        //Media should already be in protected storage at this point
        
        // Go to the next page, which is always the summary
        navigationManager.NavigateTo(FloodReportCreatePages.Summary.Url);
    }
}
