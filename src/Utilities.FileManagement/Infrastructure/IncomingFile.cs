using MediatR;
using Utilities.FileManagement.Contracts;
using Utilities.Gpg.MediatR;

namespace Utilities.FileManagement.Infrastructure;

public abstract class IncomingFile(
	IMediator mediator,
	string archiveFolderBasePath,
	string dataTransferFolderBasePath,
	string gpgPrivateKeyName,
	string gpgPrivateKeyPassword,
	string fileName,
	string gpgFileName) : FileBase(mediator,
	archiveFolderBasePath,
	dataTransferFolderBasePath), IIncomingFile
{
	public string FileName { get; } = fileName;
	public string GpgFileName { get; } = gpgFileName;
	public string GpgPrivateKeyName { get; } = gpgPrivateKeyName;
	public string GpgPrivateKeyPassword { get; } = gpgPrivateKeyPassword;
	public string DataTransferGpgFullPath => $@"{DataTransferFolderBasePath}{GpgFileName}";
	public string ArchiveFileFullPath => $@"{ArchiveFolder}{FileName}";
	public string ArchiveGpgFileFullPath => $@"{ArchiveFolder}{GpgFileName}";

	public bool DoesArchiveFileExist()
	{
		return File.Exists(ArchiveFileFullPath);
	}

	public bool DoesArchiveGpgFileExist()
	{
		return File.Exists(ArchiveGpgFileFullPath);
	}

	public async Task DecryptFile()
	{
		await Mediator.Send(new DecryptFileCommand(ArchiveGpgFileFullPath, ArchiveFileFullPath, GpgPrivateKeyName,
			GpgPrivateKeyPassword));
	}

	public async Task MoveArchiveFileToProcessedFolder()
	{
		await MoveToFolder(ArchiveFileFullPath, ArchiveProcessedFolder);
	}

	public async Task MoveArchiveFileToFailedFolder()
	{
		await MoveToFolder(ArchiveFileFullPath, ArchiveFailedFolder);
	}

	public async Task MoveArchiveGpgFileToProcessedFolder()
	{
		await MoveToFolder(ArchiveGpgFileFullPath, ArchiveProcessedFolder);
	}

	public async Task MoveArchiveGpgFileToFailedFolder()
	{
		await MoveToFolder(ArchiveGpgFileFullPath, ArchiveFailedFolder);
	}

	public async Task MoveToGpgFileToArchiveFolder()
	{
		await MoveToFolder(DataTransferGpgFullPath, ArchiveFolder);
	}
}
