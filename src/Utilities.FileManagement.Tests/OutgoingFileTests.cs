using MediatR;
using Moq;
using Utilities.FileManagement.Tests.Workflows;
using Utilities.Gpg.MediatR;
using Utilities.IoOperations.MediatR.File.CopyFile;
using Utilities.IoOperations.MediatR.File.MoveFile;

namespace Utilities.FileManagement.Tests;

public class OutgoingFileTests
{
	internal static readonly string ArchiveFolderBasePath = "OutgoingFileArchiveFolderPath\\";
	internal static readonly string DataTransferFolderBasePath = "OutgoingFileDataTransferFolderPath\\";
	internal static readonly string GpgPublicKeyName = "MyPublicKey.asc";

	private static Mock<IMediator> GetMockMediator()
	{
		Mock<IMediator> mock = new();
		return mock;
	}

	[Fact]
	public void OutgoingFileWorkflow_SetsProperties()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();

		//Act
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		//Assert

		//FileBase Properties
		Assert.True(outgoingFileWorkflow.ArchiveFolderBasePath == ArchiveFolderBasePath);
		Assert.True(outgoingFileWorkflow.DataTransferFolderBasePath == DataTransferFolderBasePath);
		Assert.True(outgoingFileWorkflow.ArchiveFolder ==
					@$"{outgoingFileWorkflow.ArchiveFolderBasePath}{DateTime.Now:MMddyyyy}\");
		Assert.True(
			outgoingFileWorkflow.ArchiveProcessedFolder == @$"{outgoingFileWorkflow.ArchiveFolder}Processed\");
		Assert.True(outgoingFileWorkflow.ArchiveFailedFolder == @$"{outgoingFileWorkflow.ArchiveFolder}Failed\");

		//OutgoingFiles Properties
		Assert.True(outgoingFileWorkflow.FileName == "File1.txt");
		Assert.True(outgoingFileWorkflow.GpgFileName == "File1.txt.gpg");

		Assert.True(outgoingFileWorkflow.GpgPublicKeyName == GpgPublicKeyName);
		Assert.True(outgoingFileWorkflow.ArchiveFileFullPath == $"{outgoingFileWorkflow.ArchiveFolder}File1.txt");
		Assert.True(
			outgoingFileWorkflow.ArchiveGpgFileFullPath == $"{outgoingFileWorkflow.ArchiveFolder}File1.txt.gpg");
		Assert.True(outgoingFileWorkflow.DataTransferGpgFullPath ==
					$"{outgoingFileWorkflow.DataTransferFolderBasePath}File1.txt.gpg");
	}

	[Fact]
	public void EncryptFile_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		//Act
		_ = outgoingFileWorkflow.EncryptFile();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<EncryptFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<EncryptFileCommand>(request =>
					request.InputFileLocation == $"{outgoingFileWorkflow.ArchiveFileFullPath}"
					&& request.OutputFileLocation == $"{outgoingFileWorkflow.ArchiveGpgFileFullPath}"
					&& request.PublicKeyName == outgoingFileWorkflow.GpgPublicKeyName),
				CancellationToken.None),
			Times.Once);
	}

	[Fact]
	public void DoArchiveFilesExist_FileExists_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		if (!Directory.Exists(outgoingFileWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(outgoingFileWorkflow.ArchiveFolder);
		}

		if (File.Exists(outgoingFileWorkflow.ArchiveFileFullPath))
		{
			File.Delete(outgoingFileWorkflow.ArchiveFileFullPath);
		}

		using (StreamWriter writer = new(outgoingFileWorkflow.ArchiveFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		bool doesExist = outgoingFileWorkflow.DoesArchiveFileExist();

		//Assert
		Assert.True(doesExist);
	}

	[Fact]
	public void DoArchiveFilesExist_FileDoesNotExist_ReturnsFalse()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		if (!Directory.Exists(outgoingFileWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(outgoingFileWorkflow.ArchiveFolder);
		}

		if (File.Exists(outgoingFileWorkflow.ArchiveFileFullPath))
		{
			File.Delete(outgoingFileWorkflow.ArchiveFileFullPath);
		}

		//Act
		bool doesExist = outgoingFileWorkflow.DoesArchiveFileExist();

		//Assert
		Assert.False(doesExist);
	}

	[Fact]
	public void DoesArchiveGpgFileExist_FileExists_ReturnsTrue()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		if (!Directory.Exists(outgoingFileWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(outgoingFileWorkflow.ArchiveFolder);
		}

		if (File.Exists(outgoingFileWorkflow.ArchiveGpgFileFullPath))
		{
			File.Delete(outgoingFileWorkflow.ArchiveGpgFileFullPath);
		}

		using (StreamWriter writer = new(outgoingFileWorkflow.ArchiveGpgFileFullPath))
		{
			writer.WriteLine("Hello World!");
		}

		//Act
		bool doesExist = outgoingFileWorkflow.DoesArchiveGpgFileExist();

		//Assert
		Assert.True(doesExist);
	}

	[Fact]
	public void DoesArchiveGpgFileExist_FileDoesNotExists_ReturnsFalse()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		if (!Directory.Exists(outgoingFileWorkflow.ArchiveFolder))
		{
			_ = Directory.CreateDirectory(outgoingFileWorkflow.ArchiveFolder);
		}

		if (File.Exists(outgoingFileWorkflow.ArchiveGpgFileFullPath))
		{
			File.Delete(outgoingFileWorkflow.ArchiveGpgFileFullPath);
		}

		//Act
		bool doesExist = outgoingFileWorkflow.DoesArchiveGpgFileExist();

		//Assert
		Assert.False(doesExist);
	}

	[Fact]
	public async Task CopyGpgFileToDataTransferFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		//Act
		await outgoingFileWorkflow.CopyGpgFileToDataTransferFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<CopyFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<CopyFileCommand>(request =>
				request.SourceFile == $"{outgoingFileWorkflow.ArchiveGpgFileFullPath}"
				&& request.DestinationFolder == $"{outgoingFileWorkflow.DataTransferFolderBasePath}"),
			CancellationToken.None), Times.Once);
	}

	[Fact]
	public void MoveArchiveFileToProcessedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		//Act
		_ = outgoingFileWorkflow.MoveArchiveFileToProcessedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
				request.SourceFile == $"{outgoingFileWorkflow.ArchiveFileFullPath}"
				&& request.DestinationFolder == $"{outgoingFileWorkflow.ArchiveProcessedFolder}"),
			CancellationToken.None), Times.Once);
	}

	[Fact]
	public void MoveArchiveGpgFileToProcessedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		//Act
		_ = outgoingFileWorkflow.MoveArchiveGpgFileToProcessedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
				request.SourceFile == $"{outgoingFileWorkflow.ArchiveGpgFileFullPath}"
				&& request.DestinationFolder == $"{outgoingFileWorkflow.ArchiveProcessedFolder}"),
			CancellationToken.None), Times.Once);
	}

	[Fact]
	public void MoveArchiveFileToFailedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		//Act
		_ = outgoingFileWorkflow.MoveArchiveFileToFailedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
				request.SourceFile == $"{outgoingFileWorkflow.ArchiveFileFullPath}"
				&& request.DestinationFolder == $"{outgoingFileWorkflow.ArchiveFailedFolder}"),
			CancellationToken.None), Times.Once);
	}

	[Fact]
	public void MoveArchiveGpgFileToFailedFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		OutgoingFileWorkflow outgoingFileWorkflow =
			new(mock.Object, ArchiveFolderBasePath, DataTransferFolderBasePath, "File1.txt", "File1.txt.gpg",
				GpgPublicKeyName);

		//Act
		_ = outgoingFileWorkflow.MoveArchiveGpgFileToFailedFolder();

		//Assert
		mock.Verify(g => g.Send(It.IsAny<MoveFileCommand>(), CancellationToken.None), Times.Exactly(1));

		mock.Verify(g => g.Send(It.Is<MoveFileCommand>(request =>
				request.SourceFile == $"{outgoingFileWorkflow.ArchiveGpgFileFullPath}"
				&& request.DestinationFolder == $"{outgoingFileWorkflow.ArchiveFailedFolder}"),
			CancellationToken.None), Times.Once);
	}
}
