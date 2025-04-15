using MediatR;
using Moq;
using Utilities.FileManagement.Contracts;
using Utilities.FileManagement.Exceptions;
using Utilities.FileManagement.Models;
using Utilities.FileManagement.Tests.Workflows;
using Utilities.Gpg.MediatR;
using Utilities.IoOperations.MediatR.File.MoveFile;

namespace Utilities.FileManagement.Tests.InfrastructureTests;

public class IncomingFilesTests
{
	internal static readonly string ArchiveFolderBasePath = "IncomingFilesArchiveFolderPath\\";
	internal static readonly string DataTransferFolderBasePath = "IncomingFilesDataTransferFolderPath\\";
	internal static readonly string GpgPrivateKeyName = "MyPublicKey.asc";
	internal static readonly string GpgPrivateKeyPassword = "password";

	private static Mock<IMediator> GetMockMediator()
	{
		Mock<IMediator> mock = new();
		return mock;
	}

	[Fact]
	public void IncomingFilesWorkflow_SetsProperties()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();

		//Act
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		//Assert

		//FileBase Properties
		Assert.True(incomingFilesWorkflow.ArchiveFolderBasePath == ArchiveFolderBasePath);
		Assert.True(incomingFilesWorkflow.DataTransferFolderBasePath == DataTransferFolderBasePath);
		Assert.True(incomingFilesWorkflow.ArchiveFolder ==
					@$"{incomingFilesWorkflow.ArchiveFolderBasePath}{DateTime.Now:MMddyyyy}\");
		Assert.True(
			incomingFilesWorkflow.ArchiveProcessedFolder == @$"{incomingFilesWorkflow.ArchiveFolder}Processed\");
		Assert.True(incomingFilesWorkflow.ArchiveFailedFolder == @$"{incomingFilesWorkflow.ArchiveFolder}Failed\");

		//IncomingFiles Properties
		Assert.True(incomingFilesWorkflow.GpgPrivateKeyName == GpgPrivateKeyName);
		Assert.True(incomingFilesWorkflow.GpgPrivateKeyPassword == GpgPrivateKeyPassword);
	}

	[Fact]
	public void GetArchiveFileFullPath_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		//Act
		string archiveFileFullPath = incomingFilesWorkflow.GetArchiveFileFullPath("File1.txt");

		//Assert
		Assert.True(archiveFileFullPath == $"{incomingFilesWorkflow.ArchiveFolder}File1.txt");
	}

	[Fact]
	public void GetArchiveGpgFileFullPath_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		//Act
		string archiveGpgFileFullPath = incomingFilesWorkflow.GetArchiveGpgFileFullPath("File1.txt");

		//Assert
		Assert.True(archiveGpgFileFullPath == $"{incomingFilesWorkflow.ArchiveFolder}File1.txt");
	}

	[Fact]
	public void DataTransferGpgFullPath_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		//Act
		string archiveGpgFileFullPath = incomingFilesWorkflow.GetDataTransferGpgFullPath("File1.txt");

		//Assert
		Assert.True(archiveGpgFileFullPath == $"{incomingFilesWorkflow.DataTransferFolderBasePath}File1.txt");
	}

	[Fact]
	public void DecryptFiles_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("File1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File3.txt.gpg");

		//Act
		_ = incomingFilesWorkflow.DecryptFiles();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<DecryptFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<DecryptFileCommand>(request =>
						request.InputFileLocation == $"{incomingFilesWorkflow.Files[i].ArchiveGpgFileFullPath}"
						&& request.OutputFileLocation == $"{incomingFilesWorkflow.Files[i].ArchiveFileFullPath}"
						&& request.PrivateKeyName == incomingFilesWorkflow.GpgPrivateKeyName
						&& request.PrivateKeyPassword == incomingFilesWorkflow.GpgPrivateKeyPassword),
					CancellationToken.None),
				Times.Once);
		}
	}

	[Fact]
	public async Task DecryptFiles_ThrowsException()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		mock.Setup(m => m.Send(It.IsAny<DecryptFileCommand>(), CancellationToken.None))
			.Throws(new Exception("Failed to decrypt because I said so."));

		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("File1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File3.txt.gpg");

		//Act & Assert
		await Assert.ThrowsAsync<DecryptionException>(() => incomingFilesWorkflow.DecryptFiles());
	}

	[Fact]
	public void DoArchiveGpgFilesExist_AllFilesExist_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("GpgFileExist1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("GpgFileExist2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("GpgFileExist3.txt.gpg");

		if (!Directory.Exists(incomingFilesWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(incomingFilesWorkflow.ArchiveFolder);
		}

		foreach (DecryptionFileDto file in incomingFilesWorkflow.Files.Where(file =>
					File.Exists(file.ArchiveGpgFileFullPath)))
		{
			File.Delete(file.ArchiveGpgFileFullPath);
		}

		foreach (DecryptionFileDto file in incomingFilesWorkflow.Files)
		{
			using (StreamWriter writer = new(file.ArchiveGpgFileFullPath))
			{
				writer.WriteLine("Hello World!");
			}
		}

		//Act
		bool doExist = incomingFilesWorkflow.DoArchiveGpgFilesExist();

		//Assert
		Assert.True(doExist);
	}

	[Fact]
	public void DoArchiveGpgFilesExist_OneFileDoesNotExist_ReturnsFalse()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("GpgFileDoesNotExist1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("GpgFileDoesNotExist2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("GpgFileDoesNotExist3.txt.gpg");

		if (!Directory.Exists(incomingFilesWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(incomingFilesWorkflow.ArchiveFolder);
		}

		foreach (DecryptionFileDto file in incomingFilesWorkflow.Files.Where(file =>
					File.Exists(file.ArchiveGpgFileFullPath)))
		{
			File.Delete(file.ArchiveGpgFileFullPath);
		}


		using (StreamWriter writer = new(incomingFilesWorkflow.Files[0].ArchiveGpgFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		using (StreamWriter writer = new(incomingFilesWorkflow.Files[1].ArchiveGpgFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		bool doExist = incomingFilesWorkflow.DoArchiveGpgFilesExist();

		//Assert
		Assert.False(doExist);
	}

	[Fact]
	public void DoArchiveFilesExist_AllFilesExist_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("ArchiveFileExist1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("ArchiveFileExistFile2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("ArchiveFileExistFile3.txt.gpg");

		if (!Directory.Exists(incomingFilesWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(incomingFilesWorkflow.ArchiveFolder);
		}

		foreach (DecryptionFileDto file in incomingFilesWorkflow.Files.Where(file =>
					File.Exists(file.ArchiveFileFullPath)))
		{
			File.Delete(file.ArchiveFileFullPath);
		}

		foreach (DecryptionFileDto file in incomingFilesWorkflow.Files)
		{
			using (StreamWriter writer = new(file.ArchiveFileFullPath))
			{
				writer.WriteLine("Hello World!");
			}
		}

		//Act
		bool doExist = incomingFilesWorkflow.DoArchiveFilesExist();

		//Assert
		Assert.True(doExist);
	}

	[Fact]
	public void DoArchiveFilesExist_OneFileDoesNotExist_ReturnsFalse()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("ArchiveFileDoesNotExist1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("ArchiveFileDoesNotExist2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("ArchiveFileDoesNotExist3.txt.gpg");

		if (!Directory.Exists(incomingFilesWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(incomingFilesWorkflow.ArchiveFolder);
		}

		foreach (DecryptionFileDto file in incomingFilesWorkflow.Files.Where(file =>
					File.Exists(file.ArchiveFileFullPath)))
		{
			File.Delete(file.ArchiveFileFullPath);
		}

		using (StreamWriter writer = new(incomingFilesWorkflow.Files[0].ArchiveFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		using (StreamWriter writer = new(incomingFilesWorkflow.Files[1].ArchiveFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		bool doExist = incomingFilesWorkflow.DoArchiveFilesExist();

		//Assert
		Assert.False(doExist);
	}

	[Fact]
	public void MoveArchiveFilesToProcessedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("File1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File3.txt.gpg");

		//Act
		_ = incomingFilesWorkflow.MoveArchiveFilesToProcessedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
					request.SourceFile == $"{incomingFilesWorkflow.Files[i].ArchiveFileFullPath}"
					&& request.DestinationFolder == $"{incomingFilesWorkflow.ArchiveProcessedFolder}"),
				CancellationToken.None), Times.Once);
		}
	}

	[Fact]
	public void MoveArchiveFilesToFailedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("File1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File3.txt.gpg");

		//Act
		_ = incomingFilesWorkflow.MoveArchiveFilesToFailedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
						request.SourceFile == $"{incomingFilesWorkflow.Files[i].ArchiveFileFullPath}"
						&& request.DestinationFolder == $"{incomingFilesWorkflow.ArchiveFailedFolder}"),
					CancellationToken.None),
				Times.Once);
		}
	}

	[Fact]
	public void MoveArchiveGpgFilesToProcessedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("File1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File3.txt.gpg");

		//Act
		_ = incomingFilesWorkflow.MoveArchiveGpgFilesToProcessedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
					request.SourceFile == $"{incomingFilesWorkflow.Files[i].ArchiveGpgFileFullPath}"
					&& request.DestinationFolder == $"{incomingFilesWorkflow.ArchiveProcessedFolder}"),
				CancellationToken.None), Times.Once);
		}
	}

	[Fact]
	public void MoveArchiveGpgFilesToFailedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("File1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File3.txt.gpg");

		//Act
		_ = incomingFilesWorkflow.MoveArchiveGpgFilesToFailedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
						request.SourceFile == $"{incomingFilesWorkflow.Files[i].ArchiveGpgFileFullPath}"
						&& request.DestinationFolder == $"{incomingFilesWorkflow.ArchiveFailedFolder}"),
					CancellationToken.None),
				Times.Once);
		}
	}

	[Fact]
	public async Task MoveGpgFilesToArchiveFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("File1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File3.txt.gpg");

		//Act
		bool isSuccessful = await incomingFilesWorkflow.MoveGpgFilesToArchiveFolder();

		//Assert
		Assert.True(isSuccessful);
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(3));

		for (int i = 0; i < 3; i++)
		{
			mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
					request.SourceFile == $"{incomingFilesWorkflow.Files[i].DataTransferGpgFileFullPath}"
					&& request.DestinationFolder == $"{incomingFilesWorkflow.ArchiveFolder}"),
				CancellationToken.None), Times.Once);
		}
	}

	[Fact]
	public async Task MoveGpgFilesToArchiveFolder_ThrowsException()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		mock.Setup(m => m.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None))
			.Throws(new Exception("Failed to move file because I said so."));

		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		incomingFilesWorkflow.AddFileToDecrypt("File1.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File2.txt.gpg");
		incomingFilesWorkflow.AddFileToDecrypt("File3.txt.gpg");

		//Act & Assert
		await Assert.ThrowsAsync<FileIoException>(() => incomingFilesWorkflow.MoveGpgFilesToArchiveFolder());
	}

	[Fact]
	public void GetGpgFilesInDataTransferFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFiles incomingFilesWorkflow =
			new IncomingFilesWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword);

		if (!Directory.Exists(incomingFilesWorkflow.DataTransferFolderBasePath))
		{
			_ = Directory.CreateDirectory(incomingFilesWorkflow.DataTransferFolderBasePath);
		}

		if (File.Exists($"{incomingFilesWorkflow.DataTransferFolderBasePath}File1.txt.gpg"))
		{
			File.Delete($"{incomingFilesWorkflow.DataTransferFolderBasePath}File1.txt.gpg");
		}

		if (File.Exists($"{incomingFilesWorkflow.DataTransferFolderBasePath}File2.txt.gpg"))
		{
			File.Delete($"{incomingFilesWorkflow.DataTransferFolderBasePath}File2.txt.gpg");
		}

		using (StreamWriter writer = new($"{incomingFilesWorkflow.DataTransferFolderBasePath}File1.txt.gpg"))
		{
			writer.WriteLine("Hello World!");
		}

		using (StreamWriter writer = new($"{incomingFilesWorkflow.DataTransferFolderBasePath}File2.txt.gpg"))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		incomingFilesWorkflow.GetGpgFilesInDataTransferFolder();

		//Assert
		Assert.True(incomingFilesWorkflow.Files.Count == 2);

		DecryptionFileDto file1 = incomingFilesWorkflow.Files[0];
		DecryptionFileDto file2 = incomingFilesWorkflow.Files[1];

		Assert.True(file1.DataTransferGpgFileFullPath ==
					$"{incomingFilesWorkflow.DataTransferFolderBasePath}File1.txt.gpg");
		Assert.True(file1.ArchiveGpgFileFullPath == $"{incomingFilesWorkflow.ArchiveFolder}File1.txt.gpg");
		Assert.True(file1.ArchiveFileFullPath == $"{incomingFilesWorkflow.ArchiveFolder}File1.txt");

		Assert.True(file2.DataTransferGpgFileFullPath ==
					$"{incomingFilesWorkflow.DataTransferFolderBasePath}File2.txt.gpg");
		Assert.True(file2.ArchiveGpgFileFullPath == $"{incomingFilesWorkflow.ArchiveFolder}File2.txt.gpg");
		Assert.True(file2.ArchiveFileFullPath == $"{incomingFilesWorkflow.ArchiveFolder}File2.txt");
	}
}
