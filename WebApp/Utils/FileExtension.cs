namespace WebApp.Utils;

public static class FileExtension
{
    /// <summary>
    /// Asynchronously reads the content of a file from the specified file path and returns it as a byte array.
    /// </summary>
    /// <param name="filePath">The path to the file to be read.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the file content as a byte array.</returns>
    public static async Task<byte[]> ReadToBytesAsync(this string filePath)
        => await File.ReadAllBytesAsync(filePath);

    /// <summary>
    /// Asynchronously reads the content of the provided file and writes it to the specified file path.
    /// </summary>
    /// <param name="file">The IFormFile object representing the file to be read.</param>
    /// <param name="filePath">The destination file path where the content will be written.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteToDiskAsync(this IFormFile file, string filePath)
    {
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);
    }
}