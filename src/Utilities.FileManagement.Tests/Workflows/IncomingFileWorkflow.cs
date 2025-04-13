using MediatR;
using Utilities.FileManagement.Contracts;
using Utilities.FileManagement.Infrastructure;

namespace Utilities.FileManagement.Tests.Workflows;

internal class IncomingFileWorkflow(
	IMediator mediator,
	string archiveFolderBasePath,
	string dataTransferFolderBasePath,
	string gpgPrivateKeyName,
	string gpgPrivateKeyPassword,
	string fileName,
	string gpgFileName)
	: IncomingFile(mediator,
		archiveFolderBasePath,
		dataTransferFolderBasePath,
		gpgPrivateKeyName,
		gpgPrivateKeyPassword,
		fileName,
		gpgFileName), IIncomingFile
{
}