using MediatR;
using Utilities.FileManagement.Contracts;
using Utilities.FileManagement.Infrastructure;

namespace Utilities.FileManagement.Tests.Workflows;

public class OutgoingFileWorkflow(
	IMediator mediator,
	string archiveFolderBasePath,
	string dataTransferFolderBasePath,
	string fileName,
	string gpgFileName,
	string gpgPublicKeyName)
	: OutgoingFile(mediator,
		archiveFolderBasePath,
		dataTransferFolderBasePath,
		fileName,
		gpgFileName,
		gpgPublicKeyName), IOutgoingFile
{
}