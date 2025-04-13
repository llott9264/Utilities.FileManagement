using Utilities.FileManagement.Models;

namespace Utilities.FileManagement.Contracts;

public interface IOutgoingFiles : IFileBase
{
	List<EncryptionFileDto> Files { get; }
	string GpgPublicKeyName { get; }
	string GetArchiveFileFullPath(string fileName);
	string GetArchiveGpgFileFullPath(string fileName);
	string GetDataTransferGpgFullPath(string fileName);
	Task<bool> EncryptFiles();
	void AddFileToEncrypt(string fileName);
	bool DoArchiveGpgFilesExist();
	bool DoArchiveFilesExist();
	Task<bool> CopyGpgFilesToDataTransferFolder();
	Task MoveArchiveFilesToProcessedFolder();
	Task MoveArchiveGpgFilesToProcessedFolder();
	Task MoveArchiveFilesToFailedFolder();
	Task MoveArchiveGpgFilesToFailedFolder();
}
