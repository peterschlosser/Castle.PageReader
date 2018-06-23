// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.IO;

namespace Castle.PageReader.Models
{
    /// <summary>
    /// A class representing a text file.
    /// </summary>
    public class PageReaderFile
    {
        public PageReaderFile(string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                var info = new FileInfo(path);
                Name = info.Name;
                Length = info.Length;
            }
        }

        public string Name { get; protected set; }
        public long Length { get; protected set; }
    }
}
