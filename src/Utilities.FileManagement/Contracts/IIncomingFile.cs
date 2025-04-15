namespace Utilities.FileManagement.Contracts;

public interface IIncomingFile : IFileBase
{
	string FileName { get; }
	string GpgFileName { get; }
	string GpgPrivateKeyName { get; }
	string GpgPrivateKeyPassword { get; }
	string DataTransferGpgFullPath { get; }
	string ArchiveFileFullPath { get; }
	string ArchiveGpgFileFullPath { get; }

	Task MoveToGpgFileToArchiveFolder();
	bool DoesArchiveGpgFileExist();
	bool DoesArchiveFileExist();
	Task DecryptFile();

	Task MoveArchiveFileToProcessedFolder();
	Task MoveArchiveFileToFailedFolder();
	Task MoveArchiveGpgFileToProcessedFolder();
	Task MoveArchiveGpgFileToFailedFolder();
}
