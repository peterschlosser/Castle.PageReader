// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using Castle.PageReader.Data;
using Castle.PageReader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Castle.PageReader
{
    /// <summary>
    /// A stateless static class presenting methods for reading text file lines
    /// one page (of count lines) at a time.
    /// </summary>
    /// <remarks>
    /// The request Count of lines sets the page length of each read request.  <see cref="PageReaderContext"/>
    /// is derived using page offsets from prior read requests providing file position
    /// page continuity even when Count varies from one request to the next.
    /// </remarks>
    public class PageReader
    {
        public static string Path { get => PageReaderRepository.Path; set => PageReaderRepository.Path = value; }

        /// <summary>
        /// Refactors PageBottom for end of file (EOF) in <see cref="PageReaderData"/>
        /// </summary>
        /// <remarks>
        /// When the <paramref name="offset"/> is at or exceeds the <paramref name="fileLength"/> 
        /// we use the EOF property to signal end of file and no more (forward) pages available.
        /// </remarks>
        internal static long PageBottom(long offset, long fileLength)
        {
            return offset >= fileLength ? PageReaderData.EOF : offset;
        }

        /// <summary>
        /// Returns list of applicable log files
        /// </summary>
        public static IEnumerable<PageReaderFile> GetFiles()
        {
            var paths = PageReaderRepository.GetFiles();
            return paths.Select(path => new PageReaderFile(path));
        }
        public static async Task<IEnumerable<PageReaderFile>> GetFilesAsync()
        {
            var paths = await PageReaderRepository.GetPathsAsync();
            return paths.Select(path => new PageReaderFile(path));
        }

        /// <summary>
        /// Reads the first N lines of the <see cref="PageReaderData"/>.
        /// </summary>
        public static PageReaderData ReadFirst(PageReaderData reader)
        {
            var path = PageReaderRepository.GetFile(reader.Id);
            var file = new FileInfo(path);
            var data = new PageReaderContext()
            {
                Path = path,
                Count = reader.Count,
                PageBottom = PageReaderData.EOF,
                Backward = false
            };
            PageReaderRepository.ReadLines(data);
            reader.PageTop = data.PageTop;
            reader.PageBottom = PageBottom(data.PageBottom, file.Length);
            reader.Lines = data.Lines;
            return reader;
        }
        public static async Task<PageReaderData> ReadFirstAsync(PageReaderData reader)
        {
            var path = await PageReaderRepository.GetPathAsync(reader.Id);
            var file = new FileInfo(path);
            var data = new PageReaderContext()
            {
                Path = path,
                Count = reader.Count,
                PageBottom = PageReaderData.EOF,
                Backward = false
            };
            await PageReaderRepository.ReadLinesAsync(data);
            reader.PageTop = data.PageTop;
            reader.PageBottom = PageBottom(data.PageBottom, file.Length);
            reader.Lines = data.Lines;
            return reader;
        }

        /// <summary>
        /// Reads the last N lines of the <see cref="PageReaderData"/>.
        /// </summary>
        public static PageReaderData ReadLast(PageReaderData reader)
        {
            var path = PageReaderRepository.GetFile(reader.Id);
            var file = new FileInfo(path);
            var data = new PageReaderContext()
            {
                Path = path,
                Count = reader.Count,
                PageTop = file.Length,
                Backward = true
            };
            PageReaderRepository.ReadLines(data);
            reader.PageTop = data.PageTop;
            reader.PageBottom = PageBottom(data.PageBottom, file.Length);
            reader.Lines = data.Lines;
            return reader;
        }
        public static async Task<PageReaderData> ReadLastAsync(PageReaderData reader)
        {
            var path = await PageReaderRepository.GetPathAsync(reader.Id);
            var file = new FileInfo(path);
            var data = new PageReaderContext()
            {
                Path = path,
                Count = reader.Count,
                PageTop = file.Length,
                Backward = true
            };
            await PageReaderRepository.ReadLinesAsync(data);
            reader.PageTop = data.PageTop;
            reader.PageBottom = PageBottom(data.PageBottom, file.Length);
            reader.Lines = data.Lines;
            return reader;
        }

        /// <summary>
        /// Reads the next (forward or back) N lines of the <see cref="PageReaderData"/>.
        /// </summary>
        public static PageReaderData ReadNext(PageReaderData reader, bool readBackwards = false)
        {
            var path = PageReaderRepository.GetFile(reader.Id);
            var file = new FileInfo(path);
            var data = new PageReaderContext()
            {
                Path = path,
                Count = reader.Count,
                PageTop = reader.PageTop,
                PageBottom = reader.PageBottom == PageReaderData.EOF ? file.Length : Math.Min(reader.PageBottom, file.Length),
                Backward = readBackwards
            };
            PageReaderRepository.ReadLines(data);
            reader.PageTop = data.PageTop;
            reader.PageBottom = PageBottom(data.PageBottom, file.Length);
            reader.Lines = data.Lines;
            return reader;
        }
        public static async Task<PageReaderData> ReadNextAsync(PageReaderData reader, bool readBackwards = false)
        {
            var path = await PageReaderRepository.GetPathAsync(reader.Id);
            var file = new FileInfo(path);
            var data = new PageReaderContext()
            {
                Path = path,
                Count = reader.Count,
                PageTop = reader.PageTop,
                PageBottom = reader.PageBottom == PageReaderData.EOF ? file.Length : Math.Min(reader.PageBottom, file.Length),
                Backward = readBackwards
            };
            await PageReaderRepository.ReadLinesAsync(data);
            reader.PageTop = data.PageTop;
            reader.PageBottom = PageBottom(data.PageBottom, file.Length);
            reader.Lines = data.Lines;
            return reader;
        }

        /// <summary>
        /// Reads the prior N lines of the <see cref="PageReaderData"/>.
        /// </summary>
        public static PageReaderData ReadPrev(PageReaderData reader)
        {
            return ReadNext(reader, true);
        }
        public static async Task<PageReaderData> ReadPrevAsync(PageReaderData reader)
        {
            return await ReadNextAsync(reader, true);
        }
    }
}
