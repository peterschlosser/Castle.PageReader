// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;

namespace Castle.PageReader.Models
{
    /// <summary>
    /// A class providing the context for file read (page) requests.
    /// </summary>
    public class PageReaderContext
    {
        /// <summary>
        /// Disk path to target text file.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Requested count of lines to read (the page length).
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Read from file in backwards direction.
        /// </summary>
        public bool Backward { get; set; }
        /// <summary>
        /// Read relative to top of page (file position of first line in page)
        /// </summary>
        public long PageTop { get; set; }
        /// <summary>
        /// Read realtive to bottom of page (file position following last line in page)
        /// </summary>
        public long PageBottom { get; set; }
        /// <summary>
        /// Resulting line data read from file
        /// </summary>
        public List<string> Lines { get; set; } = new List<string>();
    }
}
