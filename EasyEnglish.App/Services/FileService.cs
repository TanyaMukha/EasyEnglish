namespace EasyEnglish.Services;

using EasyEnglish.App.Interfaces;
using System.Text;

/// <summary>
/// Cross-platform file service implementation using MAUI APIs
/// </summary>
public class FileService : IFileService
{
    /// <summary>
    /// Saves content to a file using platform-specific methods
    /// </summary>
    public async Task<bool> SaveFileAsync(string fileName, string content, string mimeType = "application/json")
    {
        try
        {
            // Create a temporary file first
            var tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(tempFilePath, content, Encoding.UTF8);

#if ANDROID
            return await SaveFileAndroid(fileName, tempFilePath, mimeType);
#elif IOS || MACCATALYST
            return await SaveFileIOS(fileName, tempFilePath, mimeType);
#else
            // For Windows/other platforms, save directly
            var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloadsFolder = Path.Combine(downloadsPath, "Downloads");
            
            if (!Directory.Exists(downloadsFolder))
            {
                Directory.CreateDirectory(downloadsFolder);
            }

            var targetPath = Path.Combine(downloadsFolder, fileName);
            File.Copy(tempFilePath, targetPath, true);
            
            return true;
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }

#if ANDROID
    private async Task<bool> SaveFileAndroid(string fileName, string sourceFilePath, string mimeType)
    {
        try
        {
            var contentResolver = Android.App.Application.Context.ContentResolver;
            
            // Use MediaStore for Android 10+
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {
                var contentValues = new Android.Content.ContentValues();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, mimeType);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, 
                    Android.OS.Environment.DirectoryDownloads);

                var uri = contentResolver?.Insert(
                    Android.Provider.MediaStore.Downloads.ExternalContentUri,
                    contentValues);

                if (uri != null)
                {
                    using var outputStream = contentResolver?.OpenOutputStream(uri);
                    using var inputStream = File.OpenRead(sourceFilePath);
                    
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
                // For older Android versions, use traditional approach
                var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryDownloads);
                var targetPath = Path.Combine(downloadsPath?.AbsolutePath ?? "", fileName);
                
                File.Copy(sourceFilePath, targetPath, true);
                
                // Notify media scanner
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
            
            // Fallback: use Share API
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Зберегти файл",
                File = new ShareFile(sourceFilePath)
            });
            
            return true;
        }
    }
#endif

#if IOS || MACCATALYST
    private async Task<bool> SaveFileIOS(string fileName, string sourceFilePath, string mimeType)
    {
        try
        {
            // Use Share sheet on iOS
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = fileName,
                File = new ShareFile(sourceFilePath)
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

    /// <summary>
    /// Opens file picker and reads the selected file
    /// </summary>
    public async Task<string?> PickAndReadFileAsync(params string[] fileTypes)
    {
        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, fileTypes },
                    { DevicePlatform.Android, fileTypes },
                    { DevicePlatform.WinUI, fileTypes },
                    { DevicePlatform.Tizen, fileTypes },
                    { DevicePlatform.macOS, fileTypes },
                });

            var options = new PickOptions
            {
                PickerTitle = "Виберіть файл для імпорту",
                FileTypes = customFileType,
            };

            var result = await FilePicker.Default.PickAsync(options);
            
            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking file: {ex.Message}");
            return null;
        }
    }
}
