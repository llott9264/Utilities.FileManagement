using Utilities.FileManagement.Models;

namespace Utilities.FileManagement.Contracts;

public interface IIncomingFiles : IFileBase
{
	List<DecryptionFileDto> Files { get; }
	string GpgPrivateKeyName { get; }
	string GpgPrivateKeyPassword { get; }
	string GetArchiveFileFullPath(string fileName);
	string GetArchiveGpgFileFullPath(string fileName);
	string DataTransferGpgFullPath(string fileName);
	Task<bool> DecryptFiles();
	void AddFileToDecrypt(string fileName);
	bool DoArchiveGpgFilesExist();
	bool DoArchiveFilesExist();
	Task MoveArchiveFilesToProcessedFolder();
	Task MoveArchiveGpgFilesToProcessedFolder();
	Task MoveArchiveFilesToFailedFolder();
	Task MoveArchiveGpgFilesToFailedFolder();
	Task<bool> MoveGpgFilesToArchiveFolder();
	void GetGpgFilesInDataTransferFolder();
}