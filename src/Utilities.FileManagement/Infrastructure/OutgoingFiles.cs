using MediatR;
using Utilities.FileManagement.Contracts;
using Utilities.FileManagement.Exceptions;
using Utilities.FileManagement.Models;
using Utilities.Gpg.MediatR;
using Utilities.IoOperations.MediatR.File.CopyFile;

namespace Utilities.FileManagement.Infrastructure;

public abstract class OutgoingFiles(
	IMediator mediator,
	string archiveFolderBasePath,
	string dataTransferFolderBasePath,
	string gpgPublicKeyName)
	: FileBase(mediator,
		archiveFolderBasePath,
		dataTransferFolderBasePath), IOutgoingFiles
{
	public List<EncryptionFileDto> Files { get; } = [];
	public string GpgPublicKeyName { get; } = gpgPublicKeyName;

	public string GetArchiveFileFullPath(string fileName)
	{
		return Path.Combine(ArchiveFolder, fileName);
	}

	public string GetArchiveGpgFileFullPath(string fileName)
	{
		return Path.Combine(ArchiveFolder, fileName);
	}

	public string DataTransferGpgFullPath(string fileName)
	{
		return Path.Combine(DataTransferFolderBasePath, fileName);
	}

	public async Task<bool> EncryptFiles()
	{
		bool isSuccessful;
		try
		{
			foreach (EncryptionFileDto file in Files)
			{
				await Mediator.Send(new EncryptFileCommand(file.ArchiveFileFullPath,
					file.ArchiveGpgFileFullPath,
					GpgPublicKeyName));
			}

			isSuccessful = true;
		}
		catch (Exception e)
		{
			throw new EncryptionException(e.Message);
		}

		return isSuccessful;
	}

	public void AddFileToEncrypt(string fileName)
	{
		Files.Add(new EncryptionFileDto(ArchiveFolder,
			DataTransferFolderBasePath,
			fileName));
	}

	public bool DoArchiveGpgFilesExist()
	{
		return Files.Aggregate(true, (current, file) => current && File.Exists(file.ArchiveGpgFileFullPath));
	}

	public bool DoArchiveFilesExist()
	{
		return Files.Aggregate(true, (current, file) => current && File.Exists(file.ArchiveFileFullPath));
	}

	public async Task MoveArchiveFilesToProcessedFolder()
	{
		foreach (EncryptionFileDto file in Files)
		{
			await MoveToFolder(file.ArchiveFileFullPath, ArchiveProcessedFolder);
		}
	}

	public async Task MoveArchiveGpgFilesToProcessedFolder()
	{
		foreach (EncryptionFileDto file in Files)
		{
			await MoveToFolder(file.ArchiveGpgFileFullPath, ArchiveProcessedFolder);
		}
	}

	public async Task MoveArchiveFilesToFailedFolder()
	{
		foreach (EncryptionFileDto file in Files)
		{
			await MoveToFolder(file.ArchiveFileFullPath, ArchiveFailedFolder);
		}
	}

	public async Task MoveArchiveGpgFilesToFailedFolder()
	{
		foreach (EncryptionFileDto file in Files)
		{
			await MoveToFolder(file.ArchiveGpgFileFullPath, ArchiveFailedFolder);
		}
	}

	public async Task<bool> CopyGpgFilesToDataTransferFolder()
	{
		bool isSuccessful;
		try
		{
			foreach (EncryptionFileDto file in Files)
			{
				await Mediator.Send(new CopyFileCommand(file.ArchiveGpgFileFullPath, DataTransferFolderBasePath));
			}

			isSuccessful = true;
		}
		catch (Exception e)
		{
			throw new CopyFileException(
				$"Failure to copy gpg files to data transfer folder. Error Message: {e.Message}");
		}

		return isSuccessful;
	}
}