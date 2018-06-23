// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using Castle.PageReader.Models;
using System.Threading.Tasks;

namespace Castle.PageReader
{
    /// <summary>
    /// A stateless static class presenting methods for reading log (text) files
    /// in reverse order one page (of count lines) at a time.
    /// </summary>
    /// <remarks>
    /// Reading and presenting log files one page at a time is simply
    /// the reading of text files in reverse order.  Lines returned 
    /// are reversed as are top and bottom page offsets of the <see cref="PageReaderData"/>.
    /// In all other respects, the page operations are handled the same 
    /// as those in the forward-reading <see cref="PageReader"/>
    /// </remarks>
    public class LogPageReader
    {
        public static string Path { get => PageReader.Path; set => PageReader.Path = value; }

        /// <summary>
        /// Exchanges the PageTop with the PageBottom offsets in the <see cref="PageReaderData"/>.
        /// </summary>
        /// <param name="reader"></param>
        private static void ExchangePageOffsets(PageReaderData reader)
        {
            var offset = reader.PageBottom;
            reader.PageBottom = reader.PageTop;
            reader.PageTop = offset;
        }

        /// <summary>
        /// Reads the last N lines of the <see cref="PageReaderData"/>.
        /// </summary>
        public static PageReaderData ReadFirst(PageReaderData reader)
        {
            ExchangePageOffsets(reader);
            PageReader.ReadLast(reader);
            ExchangePageOffsets(reader);
            reader.Lines.Reverse();
            return reader;
        }
        public static async Task<PageReaderData> ReadFirstAsync(PageReaderData reader)
        {
            ExchangePageOffsets(reader);
            await PageReader.ReadLastAsync(reader);
            ExchangePageOffsets(reader);
            reader.Lines.Reverse();
            return reader;
        }

        /// <summary>
        /// Reads the next N lines of the <see cref="PageReaderData"/>.
        /// </summary>
        public static PageReaderData ReadNext(PageReaderData reader)
        {
            ExchangePageOffsets(reader);
            PageReader.ReadPrev(reader);
            ExchangePageOffsets(reader);
            reader.Lines.Reverse();
            return reader;
        }
        public static async Task<PageReaderData> ReadNextAsync(PageReaderData reader)
        {
            ExchangePageOffsets(reader);
            await PageReader.ReadPrevAsync(reader);
            ExchangePageOffsets(reader);
            reader.Lines.Reverse();
            return reader;
        }

        /// <summary>
        /// Reads the prior N lines of the <see cref="PageReaderData"/>.
        /// </summary>
        public static PageReaderData ReadPrev(PageReaderData reader)
        {
            ExchangePageOffsets(reader);
            PageReader.ReadNext(reader);
            ExchangePageOffsets(reader);
            reader.Lines.Reverse();
            return reader;
        }
        public static async Task<PageReaderData> ReadPrevAsync(PageReaderData reader)
        {
            ExchangePageOffsets(reader);
            await PageReader.ReadNextAsync(reader);
            ExchangePageOffsets(reader);
            reader.Lines.Reverse();
            return reader;
        }

        /// <summary>
        /// Reads the last N lines of the <see cref="PageReaderData"/>.
        /// </summary>
        public static PageReaderData ReadLast(PageReaderData reader)
        {
            ExchangePageOffsets(reader);
            PageReader.ReadFirst(reader);
            ExchangePageOffsets(reader);
            reader.Lines.Reverse();
            return reader;
        }
        public static async Task<PageReaderData> ReadLastAsync(PageReaderData reader)
        {
            ExchangePageOffsets(reader);
            await PageReader.ReadFirstAsync(reader);
            ExchangePageOffsets(reader);
            reader.Lines.Reverse();
            return reader;
        }
    }
}
