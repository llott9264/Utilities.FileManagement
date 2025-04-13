using MediatR;
using Utilities.FileManagement.Contracts;
using Utilities.FileManagement.Infrastructure;

namespace Utilities.FileManagement.Tests.Workflows;

internal class IncomingFilesWorkflow(
	IMediator mediator,
	string archiveFolderBasePath,
	string dataTransferFolderBasePath,
	string gpgPrivateKeyName,
	string gpgPrivateKeyPassword)
	: IncomingFiles(mediator,
		archiveFolderBasePath,
		dataTransferFolderBasePath,
		gpgPrivateKeyName,
		gpgPrivateKeyPassword), IIncomingFiles
{
}