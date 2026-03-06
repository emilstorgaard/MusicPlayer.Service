namespace MusicPlayer.Application.Helpers;

public static class FileHelper
{
    private const string DefaultCoverImage = "default.jpg";

    public static bool IsValidExtension(string? fileName, string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;

        var extension = Path.GetExtension(fileName).ToLower();
        return allowedExtensions.Contains(extension);
    }

    public static async Task<string> SaveFile(Stream fileStream, string originalFileName, string folderPath)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(originalFileName).ToLower();
        var filePath = Path.Combine(folderPath, fileName);
        var fullPath = GetFullPath(filePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        
        using (var destinationStream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destinationStream);
        }

        return fileName;
    }

    public static void DeleteFile(string mediaFolder, string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var fullFilePath = GetFullPath(Path.Combine(mediaFolder, filePath));
        if (!fullFilePath.EndsWith(DefaultCoverImage) && File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }
    }

    public static string GetDefaultCoverImagePath(string folderPath)
    {
        return Path.Combine(folderPath, DefaultCoverImage);
    }

    public static string GetFullPath(string relativePath)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), relativePath);
    }
}
