namespace Utilities.FileManagement.Contracts;

public interface IIncomingFile : IFileBase
{
	Task MoveToGpgFileToArchiveFolder();
	bool DoesArchiveGpgFileExist();
	bool DoesArchiveFileExist();
	Task DecryptFile();

	Task MoveArchiveFileToProcessedFolder();
	Task MoveArchiveFileToFailedFolder();
	Task MoveArchiveGpgFileToProcessedFolder();
	Task MoveArchiveGpgFileToFailedFolder();
}
