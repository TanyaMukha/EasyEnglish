namespace EasyPeasy.App.Interfaces;

/// <summary>
/// Interface for cross-platform file operations
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Saves content to a file and allows user to choose location (Android, iOS) or downloads directly (Web)
    /// </summary>
    /// <param name="fileName">Name of the file to save</param>
    /// <param name="content">Content to save</param>
    /// <param name="mimeType">MIME type of the file</param>
    /// <returns>True if file was saved successfully</returns>
    Task<bool> SaveFileAsync(string fileName, string content, string mimeType = "application/json");

    Task<bool> SaveFileBytesAsync(string fileName, byte[] data, string mimeType = "application/octet-stream");

    /// <summary>
    /// Opens file picker to select a file
    /// </summary>
    /// <param name="fileTypes">Allowed file types</param>
    /// <returns>File content or null if cancelled</returns>
    Task<string?> PickAndReadFileAsync(params string[] fileTypes);

    Task<byte[]?> PickAndReadFileBytesAsync(params string[] fileTypes);
}
