// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;

namespace Castle.PageReader.Models
{
    /// <summary>
    /// A class representing a text file page (of lines) read request-response.
    /// </summary>
    public class PageReaderData
    {
        /// <summary>
        /// Target log file name identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Desired count of log lines for page.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The file offset of the beginning of the first line in the page of log lines.
        /// </summary>
        public long PageTop { get; set; }

        /// <summary>
        /// The file offset of the end of the last line in the page of log lines.
        /// </summary>
        public long PageBottom { get; set; }

        /// <summary>
        /// The file offset value representing the beginning and end of file.
        /// </summary>
        public static readonly long EOF = 0;

        /// <summary>
        /// The log text lines representing a page.
        /// </summary>
        public List<string> Lines { get; set; } = new List<string>();
    }
}
