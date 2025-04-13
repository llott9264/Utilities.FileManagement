namespace Utilities.FileManagement.Exceptions;

public class EncryptionException(string message)
	: Exception($"Failed to Encrypt all files.  Error Message {message}");
