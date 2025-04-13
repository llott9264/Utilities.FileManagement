using MediatR;
using Moq;
using Utilities.FileManagement.Models;
using Utilities.FileManagement.Tests.Workflows;
using Utilities.Gpg.MediatR;
using Utilities.IoOperations.MediatR.File.MoveFile;

namespace Utilities.FileManagement.Tests;

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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		Assert.True(incomingFilesWorkflow.GetArchiveFileFullPath("MyFile.txt") ==
		            $"{incomingFilesWorkflow.ArchiveFolder}MyFile.txt");
		Assert.True(incomingFilesWorkflow.GetArchiveGpgFileFullPath("MyFile.txt.gpg") ==
		            $"{incomingFilesWorkflow.ArchiveFolder}MyFile.txt.gpg");
		Assert.True(incomingFilesWorkflow.GetDataTransferGpgFullPath("MyFile.txt.gpg") ==
		            $"{incomingFilesWorkflow.DataTransferFolderBasePath}MyFile.txt.gpg");
	}

	[Fact]
	public void DecryptFiles_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
	public void DoArchiveGpgFilesExist_AllFilesExist_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
	public void GetGpgFilesInDataTransferFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IncomingFilesWorkflow incomingFilesWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
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
		Assert.True($"{incomingFilesWorkflow.DataTransferFolderBasePath}File1.txt.gpg" ==
		            incomingFilesWorkflow.Files[0].DataTransferGpgFileFullPath);
		Assert.True($"{incomingFilesWorkflow.ArchiveFolder}File1.txt.gpg" ==
		            incomingFilesWorkflow.Files[0].ArchiveGpgFileFullPath);
		Assert.True($"{incomingFilesWorkflow.ArchiveFolder}File1.txt" ==
		            incomingFilesWorkflow.Files[0].ArchiveFileFullPath);

		Assert.True($"{incomingFilesWorkflow.DataTransferFolderBasePath}File2.txt.gpg" ==
		            incomingFilesWorkflow.Files[1].DataTransferGpgFileFullPath);
		Assert.True($"{incomingFilesWorkflow.ArchiveFolder}File2.txt.gpg" ==
		            incomingFilesWorkflow.Files[1].ArchiveGpgFileFullPath);
		Assert.True($"{incomingFilesWorkflow.ArchiveFolder}File2.txt" ==
		            incomingFilesWorkflow.Files[1].ArchiveFileFullPath);
	}
}