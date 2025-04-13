namespace Utilities.FileManagement.Exceptions;

public class DecryptionException(string message)
	: Exception($"Failed to Decrypt all files.  Error Message {message}");
