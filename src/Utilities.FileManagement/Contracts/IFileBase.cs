namespace Utilities.FileManagement.Contracts;

public interface IFileBase
{
	string ArchiveFolderBasePath { get; }
	string DataTransferFolderBasePath { get; }
	string ArchiveFolder { get; }
	string ArchiveProcessedFolder { get; }
	string ArchiveFailedFolder { get; }
	Task CleanUpArchiveFolder(int fileRetentionLengthInMonths);
	Task MoveToFolder(string sourceFile, string destinationFolder);
	Task CopyToFolder(string sourceFile, string destinationFolder);
	Task CreateArchiveDirectory();
	Task DeleteFilesInDataTransferFolder();
}
