namespace Utilities.FileManagement.Contracts;

public interface IOutgoingFile : IFileBase
{
	string FileName { get; }
	string GpgFileName { get; }
	string ArchiveFileFullPath { get; }
	string ArchiveGpgFileFullPath { get; }
	string DataTransferGpgFullPath { get; }
	string GpgPublicKeyName { get; }
	bool DoesArchiveFileExist();
	bool DoesArchiveGpgFileExist();
	Task EncryptFile();
	Task CopyGpgFileToDataTransferFolder();
	Task MoveArchiveFileToProcessedFolder();
	Task MoveArchiveGpgFileToProcessedFolder();
	Task MoveArchiveFileToFailedFolder();
	Task MoveArchiveGpgFileToFailedFolder();
}