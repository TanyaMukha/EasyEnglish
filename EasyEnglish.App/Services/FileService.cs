namespace EasyEnglish.Services;

using EasyEnglish.App.Interfaces;
using System.Text;

/// <summary>
/// Cross-platform file service implementation using MAUI APIs
/// </summary>
public class FileService : IFileService
{
    /// <summary>
    /// Saves text content to a file using platform-specific methods.
    /// </summary>
    public async Task<bool> SaveFileAsync(string fileName, string content, string mimeType = "application/json")
    {
        try
        {
            var tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(tempFilePath, content, Encoding.UTF8);
            return await SaveToPlatformAsync(fileName, tempFilePath, mimeType);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves raw bytes to a file using platform-specific methods.
    /// </summary>
    public async Task<bool> SaveFileBytesAsync(string fileName, byte[] data, string mimeType = "application/octet-stream")
    {
        try
        {
            var tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(tempFilePath, data);
            return await SaveToPlatformAsync(fileName, tempFilePath, mimeType);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving file bytes: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Opens a file picker and reads the selected file as a string.
    /// </summary>
    public async Task<string?> PickAndReadFileAsync(params string[] fileTypes)
    {
        try
        {
            var result = await PickFileInternalAsync(fileTypes);
            if (result is null) return null;

            using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking file: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Opens a file picker and reads the selected file as a byte array.
    /// </summary>
    public async Task<byte[]?> PickAndReadFileBytesAsync(params string[] fileTypes)
    {
        try
        {
            var result = await PickFileInternalAsync(fileTypes);
            if (result is null) return null;

            using var stream = await result.OpenReadAsync();
            using var ms     = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking file bytes: {ex.Message}");
            return null;
        }
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// Common file picker logic shared by both read methods.
    /// </summary>
    private static async Task<FileResult?> PickFileInternalAsync(string[] fileTypes)
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS,     fileTypes },
                { DevicePlatform.Android, fileTypes },
                { DevicePlatform.WinUI,   fileTypes },
                { DevicePlatform.Tizen,   fileTypes },
                { DevicePlatform.macOS,   fileTypes },
            });

        return await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Виберіть файл для імпорту",
            FileTypes   = customFileType,
        });
    }

    /// <summary>
    /// Routes a temp file to the correct platform save implementation.
    /// </summary>
    private static async Task<bool> SaveToPlatformAsync(string fileName, string tempFilePath, string mimeType)
    {
#if ANDROID
        return await SaveFileAndroid(fileName, tempFilePath, mimeType);
#elif IOS || MACCATALYST
        return await SaveFileIOS(fileName, tempFilePath, mimeType);
#else
        var downloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        if (!Directory.Exists(downloadsFolder))
            Directory.CreateDirectory(downloadsFolder);

        File.Copy(tempFilePath, Path.Combine(downloadsFolder, fileName), overwrite: true);
        return true;
#endif
    }

#if ANDROID
    private static async Task<bool> SaveFileAndroid(string fileName, string sourceFilePath, string mimeType)
    {
        try
        {
            var contentResolver = Android.App.Application.Context.ContentResolver;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {
                var contentValues = new Android.Content.ContentValues();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, mimeType);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath,
                    Android.OS.Environment.DirectoryDownloads);

                var uri = contentResolver?.Insert(
                    Android.Provider.MediaStore.Downloads.ExternalContentUri, contentValues);

                if (uri != null)
                {
                    using var outputStream = contentResolver?.OpenOutputStream(uri);
                    using var inputStream  = File.OpenRead(sourceFilePath);

                    if (outputStream != null)
                    {
                        await inputStream.CopyToAsync(outputStream);
                        await outputStream.FlushAsync();
                        return true;
                    }
                }
            }
            else
            {
                var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryDownloads);
                var targetPath = Path.Combine(downloadsPath?.AbsolutePath ?? "", fileName);

                File.Copy(sourceFilePath, targetPath, overwrite: true);

                var mediaScanIntent = new Android.Content.Intent(
                    Android.Content.Intent.ActionMediaScannerScanFile);
                mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(targetPath)));
                Android.App.Application.Context.SendBroadcast(mediaScanIntent);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Android save error: {ex.Message}");

            // Fallback: Share sheet
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Зберегти файл",
                File  = new ShareFile(sourceFilePath),
            });

            return true;
        }
    }
#endif

#if IOS || MACCATALYST
    private static async Task<bool> SaveFileIOS(string fileName, string sourceFilePath, string mimeType)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = fileName,
                File  = new ShareFile(sourceFilePath),
            });

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"iOS save error: {ex.Message}");
            return false;
        }
    }
#endif
}
