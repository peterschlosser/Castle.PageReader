# Castle.PageReader
A collection of ASP.NET Core C# classes implementing the use of the [`StreamReverseReader`](Data/StreamReverseReader.cs) class to read lines from log and text files.  These classes are optimized for use by ASP.NET Core MVC Web Applications as demonstrated by the [`LogFileReader`](/sample/LogFileReader) sample.

## [LogPageReader](LogPageReader.cs) Class
A stateless static class for reading pages of content from a log file in reverse order.
  
* Displays lines in a reverse-reading order, from the last line of the log file to the first
* Provides methods reading from First, Last, Next and Previous paging positions
* Uses the [`PageReaderData`](Models/PageReaderData.cs) model to manage file positions and paging context
* Reads log file content using the [`PageReaderRepository`](Data/PageReaderRepository.cs)
  
## [PageReader](PageReader.cs) Class
A stateless static class for reading pages of content from a text file.
  
* Displays lines in a natural forward-reading ascending order
* Provides methods reading from First, Last, Next and Previous paging positions
* Uses the [`PageReaderData`](Models/PageReaderData.cs) model to manage file positions and paging context
* Reads log file content using the [`PageReaderRepository`](Data/PageReaderRepository.cs)
  
## [PageReaderRepository](Data/PageReaderRepository.cs) Class
A stateless static class representing a folder of text files and their contents as a data repository.  Using context provided by the [`PageReaderData`](Models/PageReaderData.cs) model,

* Sets the read position of each read request
* Reads and returns lines of content in both the forward or reverse reading directions
* Provides file positions of content for subsequent read requests
* Employs the use of [`StreamReader`](https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader?view=netcore-2.1) and [`StreamReverseReader`](Data/StreamReverseReader.cs) based on read direction

## [StreamReverseReader](Data/StreamReverseReader.cs) Class
A [TextReader](https://docs.microsoft.com/en-us/dotnet/api/system.io.textreader?view=netcore-2.1) class optimized to reading lines backward from the end of a file [`Stream`](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=netcore-2.1).

* `StreamReverseReader.ReadLine()` returns the previous line from the current position of the internal file [`Stream`](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=netcore-2.1)
* `long StreamReverseReader.Position()` returns the file position of the internal [`Stream`](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=netcore-2.1)
* `long StreamReverseReader.Seek(long offset, SeekOrigin origin)` sets and returns the file position of the internal [`Stream`](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=netcore-2.1)
 
## [StreamReaderExtensions](Data/StreamReaderExtensions.cs) Class
A class extension to [`StreamReader`](https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader?view=netcore-2.1) providing methods for getting and setting file read positions within the stream.

* `long StreamReader.Position()` returns the file position of the internal [`Stream`](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=netcore-2.1)
* `long StreamReader.Seek(long offset, SeekOrigin origin)` sets and returns the file position of the internal [`Stream`](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=netcore-2.1)

