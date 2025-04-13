using MediatR;
using Utilities.FileManagement.Contracts;
using Utilities.FileManagement.Infrastructure;

namespace Utilities.FileManagement.Tests.Workflows;

internal class OutgoingFilesWorkflow(
	IMediator mediator,
	string archiveFolderBasePath,
	string dataTransferFolderBasePath,
	string gpgPublicKeyName)
	: OutgoingFiles(mediator, archiveFolderBasePath, dataTransferFolderBasePath, gpgPublicKeyName), IOutgoingFiles
{
}