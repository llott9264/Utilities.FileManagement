namespace Utilities.FileManagement.Models;

public class DecryptionFileDto(
	string archiveFolder,
	string dataTransferFolderBasePath,
	string gpgFileName)
{
	public string ArchiveFileFullPath => $@"{archiveFolder}{Path.GetFileNameWithoutExtension(gpgFileName)}";
	public string ArchiveGpgFileFullPath => $@"{archiveFolder}{gpgFileName}";
	public string DataTransferGpgFileFullPath => $@"{dataTransferFolderBasePath}{gpgFileName}";
}
