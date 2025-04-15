using MediatR;
using Moq;
using Utilities.FileManagement.Contracts;
using Utilities.FileManagement.Tests.Workflows;
using Utilities.Gpg.MediatR;
using Utilities.IoOperations.MediatR.File.MoveFile;

namespace Utilities.FileManagement.Tests.InfrastructureTests;

public class IncomingFileTests
{
	internal static readonly string ArchiveFolderBasePath = "IncomingFileArchiveFolderPath\\";
	internal static readonly string DataTransferFolderBasePath = "IncomingFileDataTransferFolderPath\\";
	internal static readonly string GpgPrivateKeyName = "MyPublicKey.asc";
	internal static readonly string GpgPrivateKeyPassword = "password";
	internal static readonly string FileName = "File1.txt";
	internal static readonly string GpgFileName = "File1.txt.gpg";

	private static Mock<IMediator> GetMockMediator()
	{
		Mock<IMediator> mock = new();
		return mock;
	}

	[Fact]
	public void IncomingFileWorkflow_SetsProperties()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();

		//Act
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		//Assert

		//FileBase Properties
		Assert.True(incomingFileWorkflow.ArchiveFolderBasePath == ArchiveFolderBasePath);
		Assert.True(incomingFileWorkflow.DataTransferFolderBasePath == DataTransferFolderBasePath);
		Assert.True(incomingFileWorkflow.ArchiveFolder ==
					@$"{incomingFileWorkflow.ArchiveFolderBasePath}{DateTime.Now:MMddyyyy}\");
		Assert.True(incomingFileWorkflow.ArchiveProcessedFolder == @$"{incomingFileWorkflow.ArchiveFolder}Processed\");
		Assert.True(incomingFileWorkflow.ArchiveFailedFolder == @$"{incomingFileWorkflow.ArchiveFolder}Failed\");

		//IncomingFile Properties
		Assert.True(incomingFileWorkflow.FileName == FileName);
		Assert.True(incomingFileWorkflow.GpgFileName == GpgFileName);
		Assert.True(incomingFileWorkflow.GpgPrivateKeyName == GpgPrivateKeyName);
		Assert.True(incomingFileWorkflow.GpgPrivateKeyPassword == GpgPrivateKeyPassword);
		Assert.True(incomingFileWorkflow.ArchiveFileFullPath == $"{incomingFileWorkflow.ArchiveFolder}{FileName}");
		Assert.True(incomingFileWorkflow.ArchiveGpgFileFullPath ==
					$"{incomingFileWorkflow.ArchiveFolder}{GpgFileName}");
		Assert.True(incomingFileWorkflow.DataTransferGpgFullPath == $"{DataTransferFolderBasePath}{GpgFileName}");
	}

	[Fact]
	public void DecryptFile_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		//Act
		_ = incomingFileWorkflow.DecryptFile();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<DecryptFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<DecryptFileCommand>(request =>
					request.InputFileLocation == $"{incomingFileWorkflow.ArchiveGpgFileFullPath}"
					&& request.OutputFileLocation == $"{incomingFileWorkflow.ArchiveFileFullPath}"
					&& request.PrivateKeyName == incomingFileWorkflow.GpgPrivateKeyName
					&& request.PrivateKeyPassword == incomingFileWorkflow.GpgPrivateKeyPassword),
				CancellationToken.None),
			Times.Once);
	}


	[Fact]
	public void DoesArchiveFileExist_FileExists_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		if (!Directory.Exists(incomingFileWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(incomingFileWorkflow.ArchiveFolder);
		}

		if (File.Exists(incomingFileWorkflow.ArchiveFileFullPath))
		{
			File.Delete(incomingFileWorkflow.ArchiveFileFullPath);
		}

		using (StreamWriter writer = new(incomingFileWorkflow.ArchiveFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		bool doesExist = incomingFileWorkflow.DoesArchiveFileExist();

		//Assert
		Assert.True(doesExist);
	}

	[Fact]
	public void DoesArchiveFileExist_FileDoesNotExist_ReturnsFalse()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		if (!Directory.Exists(incomingFileWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(incomingFileWorkflow.ArchiveFolder);
		}

		if (File.Exists(incomingFileWorkflow.ArchiveFileFullPath))
		{
			File.Delete(incomingFileWorkflow.ArchiveFileFullPath);
		}

		//Act
		bool doesExist = incomingFileWorkflow.DoesArchiveFileExist();

		//Assert
		Assert.False(doesExist);
	}

	[Fact]
	public void DoesArchiveGpgFileExist_FileExists_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		if (!Directory.Exists(incomingFileWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(incomingFileWorkflow.ArchiveFolder);
		}

		if (File.Exists(incomingFileWorkflow.ArchiveGpgFileFullPath))
		{
			File.Delete(incomingFileWorkflow.ArchiveGpgFileFullPath);
		}

		using (StreamWriter writer = new(incomingFileWorkflow.ArchiveGpgFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		bool doesExist = incomingFileWorkflow.DoesArchiveGpgFileExist();

		//Assert
		Assert.True(doesExist);
	}

	[Fact]
	public void DoesArchiveGpgFileExist_FileDoesNotExist_ReturnsFalse()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		if (!Directory.Exists(incomingFileWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(incomingFileWorkflow.ArchiveFolder);
		}

		if (File.Exists(incomingFileWorkflow.ArchiveGpgFileFullPath))
		{
			File.Delete(incomingFileWorkflow.ArchiveGpgFileFullPath);
		}

		//Act
		bool doesExist = incomingFileWorkflow.DoesArchiveGpgFileExist();

		//Assert
		Assert.False(doesExist);
	}

	[Fact]
	public void MoveArchiveFileToProcessedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		//Act
		_ = incomingFileWorkflow.MoveArchiveFileToProcessedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
				request.SourceFile == $"{incomingFileWorkflow.ArchiveFileFullPath}"
				&& request.DestinationFolder == $"{incomingFileWorkflow.ArchiveProcessedFolder}"),
			CancellationToken.None), Times.Once);
	}

	[Fact]
	public void MoveArchiveFileToFailedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		//Act
		_ = incomingFileWorkflow.MoveArchiveFileToFailedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
					request.SourceFile == $"{incomingFileWorkflow.ArchiveFileFullPath}"
					&& request.DestinationFolder == $"{incomingFileWorkflow.ArchiveFailedFolder}"),
				CancellationToken.None),
			Times.Once);
	}

	[Fact]
	public void MoveArchiveGpgFileToProcessedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		//Act
		_ = incomingFileWorkflow.MoveArchiveGpgFileToProcessedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
				request.SourceFile == $"{incomingFileWorkflow.ArchiveGpgFileFullPath}"
				&& request.DestinationFolder == $"{incomingFileWorkflow.ArchiveProcessedFolder}"),
			CancellationToken.None), Times.Once);
	}

	[Fact]
	public void MoveArchiveGpgFileToFailedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		//Act
		_ = incomingFileWorkflow.MoveArchiveGpgFileToFailedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
					request.SourceFile == $"{incomingFileWorkflow.ArchiveGpgFileFullPath}"
					&& request.DestinationFolder == $"{incomingFileWorkflow.ArchiveFailedFolder}"),
				CancellationToken.None),
			Times.Once);
	}


	[Fact]
	public void MoveGpgFileToArchiveFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IIncomingFile incomingFileWorkflow =
			new IncomingFileWorkflow(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, GpgPrivateKeyName,
				GpgPrivateKeyPassword, FileName, GpgFileName);

		//Act
		_ = incomingFileWorkflow.MoveToGpgFileToArchiveFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
					request.SourceFile == $"{incomingFileWorkflow.DataTransferGpgFullPath}"
					&& request.DestinationFolder == $"{incomingFileWorkflow.ArchiveFolder}"),
				CancellationToken.None),
			Times.Once);
	}
}
