# LogFileReader
[![Build status](https://ci.appveyor.com/api/projects/status/8rkfcmx5txm86ygx?svg=true)](https://ci.appveyor.com/project/peterschlosser/castle-pagereader)

An ASP.NET Core MVC Web Application demonstrating the use of the [`StreamReverseReader`](/peterschlosser/Castle.PageReader/src/Castle.PageReader/Data/StreamReverseReader.cs) class to display lines of log and text files through the browser.  Buttons provide next and previous paging functions.

## StreamReverseReader Class
  A text file [`StreamReader`](https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader?view=netcore-2.1) class reading content from end of file.
  
  * Ideally suited to reading log files
  * Reads lines backward from any point in the file
  * By default, reads lines starting at End of File
  * Position and Seek methods get and set read position

### Last Lines Example
The following Action method demonstrates reading the last 10 lines of the input file:
```cs
public class HomeController : Controller
{
    public string LastLinesExample()
    {
        var lines = new List<string>();
        using (var reader = new StreamReverseReader(@"Logs/logfile.txt"))
        {
            while (!reader.EndOfStream && lines.Count < 10)
            {
                lines.Add(reader.ReadLine());
            }
        }
        return string.Join("\n", lines);
    }
}
```
Using [logfile.txt](/sample/LogFileReader/Logs/logfile.txt) as input, the result of `LastLinesExample()` looks something like:
```
0025 2018-05-02 16:21:52,449 DEBUG Lorem ipsum lastum line-um.
0024 2018-05-02 16:21:52,314 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore.
0023 2018-05-02 16:21:52,314 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.
0022 2018-05-02 16:21:52,313 INFO  Lorem ipsum.
0021 2018-05-02 16:21:52,313 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore.
0020 2018-05-02 16:21:50,516 INFO  Lorem ipsum dolor sit amet, consectetur.
0019 2018-05-02 16:21:49,954 ERROR Lorem ipsum dolor.
0018 2018-05-02 16:21:49,832 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna.
0017 2018-05-02 16:21:49,831 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing.
0016 2018-05-02 16:21:49,831 INFO  Lorem ipsum dolor sit.
```

### Middle Lines Example
The following Action method demonstrates reading lines from a random position of the input file.  In this case, somewhere near the middle:
```cs
public class HomeController : Controller
{
    public string MiddleLinesExample()
    {
        var path = @"Logs/logfile.txt";
        var top = 0L;
        var bottom = 0L;
        var start = (new System.IO.FileInfo(path)).Length / 2;  // middle of file

        var lines = new List<string>();
        using (var reader = new System.IO.StreamReverseReader(path))
        {
            reader.Seek(start);         // set start position
            reader.ReadLine();          // read and discard (potentially) partial line
            top = reader.Position();    // position of first line in list
            while (!reader.EndOfStream && lines.Count < 10)
            {
                lines.Add(reader.ReadLine());
            }
            bottom = reader.Position(); // position after last line in last
        }

        var result = $"Read position first line: {top}\n";
        result += $"Read position after last line: {bottom}\n";
        result += $"Number of lines read: {lines.Count()}\n";
        result += string.Join("\n", lines);
        return result;
    }
}
```

Using [logfile.txt](/sample/LogFileReader/Logs/logfile.txt) as input, the result of of `MiddleLinesExample()` looks something like:
```
Read position first line: 1377
Read position after last line: 318
Number of lines read: 10
0013 2018-05-02 16:21:49,621 DEBUG Lorem ipsum dolor sit amet.
0012 2018-05-02 16:21:49,620 INFO  Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore.
0011 2018-05-02 16:21:49,620 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.
0010 2018-05-02 16:21:49,300 DEBUG Lorem ipsum dolor sit amet.
0009 2018-05-02 16:21:49,299 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore.
0008 2018-05-02 16:21:49,299 INFO  Lorem ipsum dolor sit amet, consectetur.
0007 2018-05-02 16:21:49,299 DEBUG Lorem ipsum dolor.
0006 2018-05-02 16:21:49,043 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna.
0005 2018-05-02 16:21:49,042 DEBUG Lorem ipsum dolor sit amet, consectetur adipiscing.
0004 2018-05-02 16:21:49,020 INFO  Lorem ipsum dolor sit.
```

