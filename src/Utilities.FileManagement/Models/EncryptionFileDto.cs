namespace Utilities.FileManagement.Models;

public class EncryptionFileDto(
	string archiveFolder,
	string dataTransferFolderBasePath,
	string fileName)
{
	public string ArchiveFileFullPath => $@"{archiveFolder}{fileName}";
	public string ArchiveGpgFileFullPath => $@"{archiveFolder}{fileName}.gpg";
	public string DataTransferGpgFileFullPath => $@"{dataTransferFolderBasePath}{fileName}.gpg";
}
