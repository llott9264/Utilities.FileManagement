using MediatR;
using Utilities.FileManagement.Contracts;
using Utilities.FileManagement.Exceptions;
using Utilities.FileManagement.Models;
using Utilities.Gpg.MediatR;

namespace Utilities.FileManagement.Infrastructure;

public abstract class IncomingFiles(
	IMediator mediator,
	string archiveFolderBasePath,
	string dataTransferFolderBasePath,
	string gpgPrivateKeyName,
	string gpgPrivateKeyPassword)
	: FileBase(mediator,
		archiveFolderBasePath,
		dataTransferFolderBasePath), IIncomingFiles
{
	public List<DecryptionFileDto> Files { get; } = [];

	public string GpgPrivateKeyName { get; } = gpgPrivateKeyName;
	public string GpgPrivateKeyPassword { get; } = gpgPrivateKeyPassword;

	public string GetArchiveFileFullPath(string fileName)
	{
		return Path.Combine(ArchiveFolder, fileName);
	}

	public string GetArchiveGpgFileFullPath(string fileName)
	{
		return Path.Combine(ArchiveFolder, fileName);
	}

	public string GetDataTransferGpgFullPath(string fileName)
	{
		return Path.Combine(DataTransferFolderBasePath, fileName);
	}

	public async Task<bool> DecryptFiles()
	{
		bool isSuccessful;
		try
		{
			foreach (DecryptionFileDto file in Files)
			{
				await Mediator.Send(new DecryptFileCommand(file.ArchiveGpgFileFullPath,
					file.ArchiveFileFullPath,
					GpgPrivateKeyName, GpgPrivateKeyPassword));
			}

			isSuccessful = true;
		}
		catch (Exception e)
		{
			throw new DecryptionException(e.Message);
		}

		return isSuccessful;
	}

	public void AddFileToDecrypt(string fileName)
	{
		Files.Add(new DecryptionFileDto(ArchiveFolder,
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
		foreach (DecryptionFileDto file in Files)
		{
			await MoveToFolder(file.ArchiveFileFullPath, ArchiveProcessedFolder);
		}
	}

	public async Task MoveArchiveGpgFilesToProcessedFolder()
	{
		foreach (DecryptionFileDto file in Files)
		{
			await MoveToFolder(file.ArchiveGpgFileFullPath, ArchiveProcessedFolder);
		}
	}

	public async Task MoveArchiveFilesToFailedFolder()
	{
		foreach (DecryptionFileDto file in Files)
		{
			await MoveToFolder(file.ArchiveFileFullPath, ArchiveFailedFolder);
		}
	}

	public async Task MoveArchiveGpgFilesToFailedFolder()
	{
		foreach (DecryptionFileDto file in Files)
		{
			await MoveToFolder(file.ArchiveGpgFileFullPath, ArchiveFailedFolder);
		}
	}

	public async Task<bool> MoveGpgFilesToArchiveFolder()
	{
		bool isSuccessful;
		try
		{
			foreach (DecryptionFileDto file in Files)
			{
				await MoveToFolder(file.DataTransferGpgFileFullPath, ArchiveFolder);
			}

			isSuccessful = true;
		}
		catch (Exception e)
		{
			throw new CopyFileException($"Failure to copy gpg files to archive folder. Error Message: {e.Message}");
		}

		return isSuccessful;
	}

	public void GetGpgFilesInDataTransferFolder()
	{
		List<FileInfo> files = new DirectoryInfo(DataTransferFolderBasePath).GetFiles()
			.Where(f => f.Extension == ".gpg")
			.OrderBy(f => f.CreationTime)
			.ToList();

		files.ForEach(f => AddFileToDecrypt(f.Name));
	}
}