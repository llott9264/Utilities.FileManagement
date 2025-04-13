using MediatR;
using Moq;
using Utilities.FileManagement.Exceptions;
using Utilities.FileManagement.Models;
using Utilities.FileManagement.Tests.Workflows;
using Utilities.Gpg.MediatR;
using Utilities.IoOperations.MediatR.File.CopyFile;
using Utilities.IoOperations.MediatR.File.MoveFile;

namespace Utilities.FileManagement.Tests.InfrastructureTests;

public class OutgoingFilesTests
{
	internal static readonly string ArchiveFolderBasePath = "OutgoingFilesArchiveFolderPath\\";
	internal static readonly string DataTransferFolderBasePath = "OutgoingFilesDataTransferFolderPath\\";
	internal static readonly string GpgPublicKeyName = "MyPublicKey.asc";

	private static Mock<IMediator> GetMockMediator()
	{
		Mock<IMediator> mock = new();
		return mock;
	}

	[Fact]
	public void OutGoingFilesWorkflow_SetsProperties()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();

		//Act
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);

		//Assert

		//FileBase Properties
		Assert.True(outgoingFilesWorkflow.ArchiveFolderBasePath == ArchiveFolderBasePath);
		Assert.True(outgoingFilesWorkflow.DataTransferFolderBasePath == DataTransferFolderBasePath);
		Assert.True(outgoingFilesWorkflow.ArchiveFolder ==
					@$"{outgoingFilesWorkflow.ArchiveFolderBasePath}{DateTime.Now:MMddyyyy}\");
		Assert.True(
			outgoingFilesWorkflow.ArchiveProcessedFolder == @$"{outgoingFilesWorkflow.ArchiveFolder}Processed\");
		Assert.True(outgoingFilesWorkflow.ArchiveFailedFolder == @$"{outgoingFilesWorkflow.ArchiveFolder}Failed\");

		//OutgoingFiles Properties
		Assert.True(outgoingFilesWorkflow.GpgPublicKeyName == GpgPublicKeyName);
	}

	[Fact]
	public void MoveArchiveFilesToProcessedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("File1.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File2.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File3.txt.gpg");

		//Act
		_ = outgoingFilesWorkflow.MoveArchiveFilesToProcessedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
					request.SourceFile == $"{outgoingFilesWorkflow.Files[i].ArchiveFileFullPath}"
					&& request.DestinationFolder == $"{outgoingFilesWorkflow.ArchiveProcessedFolder}"),
				CancellationToken.None), Times.Once);
		}
	}

	[Fact]
	public void MoveArchiveFilesToFailedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("File1.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File2.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File3.txt.gpg");

		//Act
		_ = outgoingFilesWorkflow.MoveArchiveFilesToFailedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
						request.SourceFile == $"{outgoingFilesWorkflow.Files[i].ArchiveFileFullPath}"
						&& request.DestinationFolder == $"{outgoingFilesWorkflow.ArchiveFailedFolder}"),
					CancellationToken.None),
				Times.Once);
		}
	}

	[Fact]
	public void MoveArchiveGpgFilesToProcessedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("File1.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File2.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File3.txt.gpg");

		//Act
		_ = outgoingFilesWorkflow.MoveArchiveGpgFilesToProcessedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
					request.SourceFile == $"{outgoingFilesWorkflow.Files[i].ArchiveGpgFileFullPath}"
					&& request.DestinationFolder == $"{outgoingFilesWorkflow.ArchiveProcessedFolder}"),
				CancellationToken.None), Times.Once);
		}
	}

	[Fact]
	public void MoveArchiveGpgFilesToFailedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("File1.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File2.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File3.txt.gpg");

		//Act
		_ = outgoingFilesWorkflow.MoveArchiveGpgFilesToFailedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
						request.SourceFile == $"{outgoingFilesWorkflow.Files[i].ArchiveGpgFileFullPath}"
						&& request.DestinationFolder == $"{outgoingFilesWorkflow.ArchiveFailedFolder}"),
					CancellationToken.None),
				Times.Once);
		}
	}

	[Fact]
	public void EncryptFiles_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("File1.txt");
		outgoingFilesWorkflow.AddFileToEncrypt("File2.txt");
		outgoingFilesWorkflow.AddFileToEncrypt("File3.txt");

		//Act
		_ = outgoingFilesWorkflow.EncryptFiles();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<EncryptFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<EncryptFileCommand>(request =>
						request.InputFileLocation == $"{outgoingFilesWorkflow.Files[i].ArchiveFileFullPath}"
						&& request.OutputFileLocation == $"{outgoingFilesWorkflow.Files[i].ArchiveGpgFileFullPath}"
						&& request.PublicKeyName == outgoingFilesWorkflow.GpgPublicKeyName),
					CancellationToken.None),
				Times.Once);
		}
	}

	[Fact]
	public async Task EncryptFiles_ThrowsException()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		mock.Setup(m => m.Send(It.IsAny<EncryptFileCommand>(), CancellationToken.None))
			.Throws(new Exception("Failed to encrypt because I said so."));

		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("File1.txt");
		outgoingFilesWorkflow.AddFileToEncrypt("File2.txt");
		outgoingFilesWorkflow.AddFileToEncrypt("File3.txt");

		//Act & Assert
		await Assert.ThrowsAsync<EncryptionException>(() => outgoingFilesWorkflow.EncryptFiles());
	}

	[Fact]
	public void GetArchiveFileFullPath_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);

		//Act
		string archiveFileFullPath = outgoingFilesWorkflow.GetArchiveFileFullPath("File1.txt");

		//Assert
		Assert.True(archiveFileFullPath == $"{outgoingFilesWorkflow.ArchiveFolder}File1.txt");
	}

	[Fact]
	public void GetArchiveGpgFileFullPath_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);

		//Act
		string archiveGpgFileFullPath = outgoingFilesWorkflow.GetArchiveGpgFileFullPath("File1.txt");

		//Assert
		Assert.True(archiveGpgFileFullPath == $"{outgoingFilesWorkflow.ArchiveFolder}File1.txt");
	}

	[Fact]
	public void GetDataTransferGpgFullPath_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);

		//Act
		string archiveGpgFileFullPath = outgoingFilesWorkflow.GetDataTransferGpgFullPath("File1.txt");

		//Assert
		Assert.True(archiveGpgFileFullPath == $"{outgoingFilesWorkflow.DataTransferFolderBasePath}File1.txt");
	}

	[Fact]
	public async Task CopyGpgFilesToDataTransferFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("File1.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File2.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File3.txt.gpg");

		//Act
		bool isSuccessful = await outgoingFilesWorkflow.CopyGpgFilesToDataTransferFolder();

		//Assert
		Assert.True(isSuccessful);
		mock.Verify(g => g.Send(It.IsAny<CopyFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<CopyFileCommand>(request =>
					request.SourceFile == $"{outgoingFilesWorkflow.Files[i].ArchiveGpgFileFullPath}"
					&& request.DestinationFolder == $"{outgoingFilesWorkflow.DataTransferFolderBasePath}"),
				CancellationToken.None), Times.Once);
		}
	}

	[Fact]
	public async Task CopyGpgFilesToDataTransferFolder_ThrowsException()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		mock.Setup(m => m.Send(It.IsAny<CopyFileCommand>(), CancellationToken.None))
			.Throws(new Exception("Failed to copy file because I said so."));

		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("File1.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File2.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("File3.txt.gpg");

		//Act & Assert
		await Assert.ThrowsAsync<FileIoException>(() => outgoingFilesWorkflow.CopyGpgFilesToDataTransferFolder());
	}

	[Fact]
	public void DoArchiveGpgFilesExist_AllFilesExist_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("GpgFileExist1.txt");
		outgoingFilesWorkflow.AddFileToEncrypt("GpgFileExist2.txt");
		outgoingFilesWorkflow.AddFileToEncrypt("GpgFileExist3.txt");

		if (!Directory.Exists(outgoingFilesWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(outgoingFilesWorkflow.ArchiveFolder);
		}

		foreach (EncryptionFileDto file in outgoingFilesWorkflow.Files.Where(file =>
					File.Exists(file.ArchiveGpgFileFullPath)))
		{
			File.Delete(file.ArchiveGpgFileFullPath);
		}

		foreach (EncryptionFileDto file in outgoingFilesWorkflow.Files)
		{
			using (StreamWriter writer = new(file.ArchiveGpgFileFullPath))
			{
				writer.WriteLine("Hello World!");
			}
		}

		//Act
		bool doExist = outgoingFilesWorkflow.DoArchiveGpgFilesExist();

		//Assert
		Assert.True(doExist);
	}

	[Fact]
	public void DoArchiveGpgFilesExist_OneFileDoesNotExist_ReturnsFalse()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("GpgFileDoesNotExist1.txt");
		outgoingFilesWorkflow.AddFileToEncrypt("GpgFileDoesNotExist2.txt");
		outgoingFilesWorkflow.AddFileToEncrypt("GpgFileDoesNotExist3.txt");

		if (!Directory.Exists(outgoingFilesWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(outgoingFilesWorkflow.ArchiveFolder);
		}

		foreach (EncryptionFileDto file in outgoingFilesWorkflow.Files.Where(file =>
					File.Exists(file.ArchiveGpgFileFullPath)))
		{
			File.Delete(file.ArchiveGpgFileFullPath);
		}


		using (StreamWriter writer = new(outgoingFilesWorkflow.Files[0].ArchiveGpgFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		using (StreamWriter writer = new(outgoingFilesWorkflow.Files[1].ArchiveGpgFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		bool doExist = outgoingFilesWorkflow.DoArchiveGpgFilesExist();

		//Assert
		Assert.False(doExist);
	}

	[Fact]
	public void DoArchiveFilesExist_AllFilesExist_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("ArchiveFileExist1.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("ArchiveFileExistFile2.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("ArchiveFileExistFile3.txt.gpg");

		if (!Directory.Exists(outgoingFilesWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(outgoingFilesWorkflow.ArchiveFolder);
		}

		foreach (EncryptionFileDto file in outgoingFilesWorkflow.Files.Where(file =>
					File.Exists(file.ArchiveFileFullPath)))
		{
			File.Delete(file.ArchiveFileFullPath);
		}

		foreach (EncryptionFileDto file in outgoingFilesWorkflow.Files)
		{
			using (StreamWriter writer = new(file.ArchiveFileFullPath))
			{
				writer.WriteLine("Hello World!");
			}
		}

		//Act
		bool doExist = outgoingFilesWorkflow.DoArchiveFilesExist();

		//Assert
		Assert.True(doExist);
	}

	[Fact]
	public void DoArchiveFilesExist_OneFileDoesNotExist_ReturnsFalse()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFilesWorkflow outgoingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPublicKeyName);
		outgoingFilesWorkflow.AddFileToEncrypt("ArchiveFileDoesNotExist1.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("ArchiveFileDoesNotExist2.txt.gpg");
		outgoingFilesWorkflow.AddFileToEncrypt("ArchiveFileDoesNotExist3.txt.gpg");

		if (!Directory.Exists(outgoingFilesWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(outgoingFilesWorkflow.ArchiveFolder);
		}

		foreach (EncryptionFileDto file in outgoingFilesWorkflow.Files.Where(file =>
					File.Exists(file.ArchiveFileFullPath)))
		{
			File.Delete(file.ArchiveFileFullPath);
		}

		using (StreamWriter writer = new(outgoingFilesWorkflow.Files[0].ArchiveFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		using (StreamWriter writer = new(outgoingFilesWorkflow.Files[1].ArchiveFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		bool doExist = outgoingFilesWorkflow.DoArchiveFilesExist();

		//Assert
		Assert.False(doExist);
	}
}
