using MediatR;
using Moq;
using Utilities.FileManagement.Tests.Workflows;
using Utilities.IoOperations.MediatR.Directory.CleanUpDirectory;
using Utilities.IoOperations.MediatR.Directory.CreateDirectory;
using Utilities.IoOperations.MediatR.Directory.DeleteFiles;

namespace Utilities.FileManagement.Tests;

public class FileBaseTests
{
	private const string ArchiveFolderBasePath = "MyArchiveFolderPath\\";
	private const string DataTransferFolderBasePath = "MyDataTransferFolderPath\\";
	private const string GpgPrivateKeyName = "MyPublicKey.asc";
	private const string GpgPrivateKeyPassword = "password";
	private readonly string _folderName = DateTime.Now.ToString("MMddyyyy");

	private static Mock<IMediator> GetMockMediator()
	{
		Mock<IMediator> mock = new();
		return mock;
	}

	[Fact]
	public void CleanUpArchiveFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IncomingFileWorkflow incomingFileWorkflow = new(mock.Object,
			ArchiveFolderBasePath,
			DataTransferFolderBasePath,
			GpgPrivateKeyName,
			GpgPrivateKeyPassword,
			"FileName.txt.gpg",
			"FileName.txt"
		);

		//Act
		_ = incomingFileWorkflow.CleanUpArchiveFolder(-13);

		//Assert
		mock.Verify(g => g.Send(It.Is<CleanUpDirectoryCommand>(request =>
			request.Directory.FullName == new DirectoryInfo(ArchiveFolderBasePath).FullName
			&& request.RetentionLengthInMonths == -13
			&& request.IsBaseFolder == true), CancellationToken.None), Times.Exactly(1));
	}

	[Fact]
	public void CreateArchiveDirectory_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IncomingFileWorkflow incomingFileWorkflow = new(mock.Object,
			ArchiveFolderBasePath,
			DataTransferFolderBasePath,
			GpgPrivateKeyName,
			GpgPrivateKeyPassword,
			"FileName.txt.gpg",
			"FileName.txt"
		);

		//Act
		_ = incomingFileWorkflow.CreateArchiveDirectory();

		//Assert
		mock.Verify(g => g.Send(It.Is<CreateDirectoryCommand>(request =>
			request.Folder == $"{ArchiveFolderBasePath}{_folderName}\\"), CancellationToken.None), Times.Exactly(1));
	}

	[Fact]
	public void DeleteFilesInDataTransferFolder_MethodCallsCorrectly()
	{
		//Arrange
		Mock<IMediator> mock = GetMockMediator();
		IncomingFileWorkflow incomingFileWorkflow = new(mock.Object,
			ArchiveFolderBasePath,
			DataTransferFolderBasePath,
			GpgPrivateKeyName,
			GpgPrivateKeyPassword,
			"FileName.txt.gpg",
			"FileName.txt"
		);

		//Act
		_ = incomingFileWorkflow.DeleteFilesInDataTransferFolder();

		//Assert
		mock.Verify(g => g.Send(It.Is<DeleteFilesCommand>(request =>
				request.Directory.FullName == new DirectoryInfo(DataTransferFolderBasePath).FullName),
			CancellationToken.None), Times.Exactly(1));
	}
}
