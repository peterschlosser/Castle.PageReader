// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using Castle.PageReader.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Castle.PageReader.Data
{
    /// <summary>
    /// A stateless static class representing a folder of text files and their contents as
    /// a data repository.
    /// </summary>
    public class PageReaderRepository
    {
        private static int DefaultBufferSize => 4096;

        /// <summary>
        /// The root path containing the collection of text files.
        /// </summary>
        public static string Path { get; set; } = "Logs";  // Castle.FileLogger default

        /// <summary>
        /// Returns the file path of the specified text filename found in the <see cref="Path"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetFile(string name)
        {
            var files = GetFiles();
            return files
                .Where(path => path.EndsWith("\\" + name))
                .FirstOrDefault();
        }
        public static async Task<string> GetPathAsync(string name)
        {
            var files = await GetPathsAsync();
            return files
                .Where(path => path.EndsWith("\\" + name))
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns the file paths of all (*.txt) files within the <see cref="Path"/>
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetFiles()
        {
            var filepath = Path;
            return Directory.GetFiles(filepath)
                .Where(path => path.EndsWith(".txt"));
        }
        public static async Task<IEnumerable<string>> GetPathsAsync()
        {
            var filepath = Path;
            return await Task.Run(() =>
            {
                return Directory.GetFiles(filepath)
                    .Where(path => path.EndsWith(".txt"));
            });
        }

        /// <summary>
        /// Reads the lines requested by <see cref="PageReaderContext"/>.
        /// </summary>
        /// <remarks>
        /// Reading (lines from a file) forward is simple given <see cref="StreamReader"/>
        /// naturally reads from files in the forward direction.  Reading files 
        /// backward involves the specialized class <see cref="StreamReverseReader"/> 
        /// returning lines in reverse order.  In both cases, file positions in 
        /// and out provide context for the next/previous contiguous file read.
        /// </remarks>
        public static void ReadLines([In, Out]PageReaderContext request)
        {
            request.Lines.Clear();

            if (request.Backward)
            {
                using (var reader = new StreamReverseReader(request.Path))
                {
                    request.PageBottom = reader.Seek(request.PageTop, origin: SeekOrigin.Begin);
                    while (!reader.EndOfStream && request.Lines.Count < request.Count)
                    {
                        request.Lines.Add(reader.ReadLine());
                    }
                    request.PageTop = reader.Position();
                    request.Lines.Reverse();
                }
            }
            else
            {
                using (var reader = new StreamReader(File.Open(request.Path, FileMode.Open)))
                {
                    request.PageTop = reader.Seek(request.PageBottom, origin: SeekOrigin.Begin);
                    while (!reader.EndOfStream && request.Lines.Count < request.Count)
                    {
                        request.Lines.Add(reader.ReadLine());
                    }
                    request.PageBottom = reader.Position();
                }
            }
        }
        public static async Task ReadLinesAsync([In, Out]PageReaderContext request)
        {
            request.Lines.Clear();

            if (request.Backward)
            {
                using (var reader = new StreamReverseReader(request.Path))
                {
                    request.PageBottom = reader.Seek(request.PageTop, origin: SeekOrigin.Begin);
                    while (!reader.EndOfStream && request.Lines.Count < request.Count)
                    {
                        request.Lines.Add(await reader.ReadLineAsync());
                    }
                    request.PageTop = reader.Position();
                    request.Lines.Reverse();
                }
            }
            else
            {
                using (var reader = new StreamReader(File.Open(request.Path, FileMode.Open)))
                {
                    request.PageTop = reader.Seek(request.PageBottom, origin: SeekOrigin.Begin);
                    while (!reader.EndOfStream && request.Lines.Count < request.Count)
                    {
                        request.Lines.Add(await reader.ReadLineAsync());
                    }
                    request.PageBottom = reader.Position();
                }
            }
        }
    }
}
