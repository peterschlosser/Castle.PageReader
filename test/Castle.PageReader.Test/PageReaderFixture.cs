using Castle.PageReader.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Castle.PageReader.Test
{
    public class PageReaderFixture : IDisposable
    {
        private readonly string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        public PageReaderFixture()
        {
            TempPath = Path.GetTempFileName() + "_";
            TestName = Guid.NewGuid().ToString() + ".txt";
            PrepareTextFile();
            FileLength = (new FileInfo(Path.Combine(TempPath, TestName))).Length;

            PageReaderRepository.Path = TempPath;
        }

        public string TempPath { get; protected set; }
        public string TestName { get; protected set; }
        public long FileLength { get; protected set; }
        public int LineCount { get; protected set; } = 25;

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(TempPath))
                {
                    Directory.Delete(TempPath, true);
                }
            }
            catch
            {
                // ignored
            }
        }

        public string Text(int number)
        {
            return string.Format("{0:D4} {1}", number, LoremIpsum);
        }

        public string TextLine(int number)
        {
            return string.Format("{0}\r\n", Text(number));
        }

        void PrepareTextFile()
        {
            Directory.CreateDirectory(TempPath);
            var lines = new List<string>();
            for (var i = 1; i <= LineCount; i++)
            {
                lines.Add(Text(i));
            }
            File.WriteAllLines(Path.Combine(TempPath, TestName), lines);
        }
    }
}
