using MediatR;
using Utilities.FileManagement.Contracts;
using Utilities.Gpg.MediatR;
using Utilities.IoOperations.MediatR.File.CopyFile;

namespace Utilities.FileManagement.Infrastructure;

public abstract class OutgoingFile(
	IMediator mediator,
	string archiveFolderBasePath,
	string dataTransferFolderBasePath,
	string fileName,
	string gpgFileName,
	string gpgPublicKeyName) : FileBase(mediator, archiveFolderBasePath, dataTransferFolderBasePath), IOutgoingFile
{
	public string FileName { get; } = fileName;
	public string GpgFileName { get; } = gpgFileName;
	public string GpgPublicKeyName { get; } = gpgPublicKeyName;
	public string ArchiveFileFullPath => $@"{ArchiveFolder}{FileName}";
	public string ArchiveGpgFileFullPath => $@"{ArchiveFolder}{GpgFileName}";
	public string DataTransferGpgFullPath => $@"{DataTransferFolderBasePath}{GpgFileName}";

	public async Task EncryptFile()
	{
		await Mediator.Send(new EncryptFileCommand(ArchiveFileFullPath, ArchiveGpgFileFullPath, GpgPublicKeyName));
	}

	public bool DoesArchiveFileExist()
	{
		return File.Exists(ArchiveFileFullPath);
	}

	public bool DoesArchiveGpgFileExist()
	{
		return File.Exists(ArchiveGpgFileFullPath);
	}

	public async Task CopyGpgFileToDataTransferFolder()
	{
		await Mediator.Send(new CopyFileCommand(ArchiveGpgFileFullPath, DataTransferFolderBasePath));
	}

	public async Task MoveArchiveFileToProcessedFolder()
	{
		await MoveToFolder(ArchiveFileFullPath, ArchiveProcessedFolder);
	}

	public async Task MoveArchiveGpgFileToProcessedFolder()
	{
		await MoveToFolder(ArchiveGpgFileFullPath, ArchiveProcessedFolder);
	}

	public async Task MoveArchiveFileToFailedFolder()
	{
		await MoveToFolder(ArchiveFileFullPath, ArchiveFailedFolder);
	}

	public async Task MoveArchiveGpgFileToFailedFolder()
	{
		await MoveToFolder(ArchiveGpgFileFullPath, ArchiveFailedFolder);
	}
}