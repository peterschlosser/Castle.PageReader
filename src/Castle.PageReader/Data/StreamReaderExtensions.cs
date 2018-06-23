// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Reflection;
using System.Text;

namespace System.IO
{
    /// <summary>
    /// A class extension to <see cref="StreamReader"/> providing methods 
    /// for getting and setting file read positions within the stream.
    /// </summary>
    public static class StreamReaderExtensions
    {
        /// <remarks>
        /// Fallback property base names give us backward compatibility to ASP.NET Core 1.0.
        /// </remarks>
        private static readonly FieldInfo charPosField =
            typeof(StreamReader).GetField("_charPos", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? typeof(StreamReader).GetField("charPos", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? throw new System.ArgumentNullException("charPos", "Failure resolving property StreamReader.charPos in StreamReaderExtensions class.");
        private static readonly FieldInfo byteLenField = 
            typeof(StreamReader).GetField("_byteLen", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? typeof(StreamReader).GetField("byteLen", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? throw new System.ArgumentNullException("byteLen", "Failure resolving property StreamReader.byteLen in StreamReaderExtensions class.");
        private static readonly FieldInfo charBufferField = 
            typeof(StreamReader).GetField("_charBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? typeof(StreamReader).GetField("charBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? throw new System.ArgumentNullException("charBuffer", "Failure resolving property StreamReader.charBuffer in StreamReaderExtensions class.");

        /// <summary>
        /// Gets the position of the internal <see cref="Stream"/>.
        /// </summary>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <returns>The current position within the stream.</returns>
        public static long Position(this StreamReader reader)
        {
            long position = reader.BaseStream.Position - (int)byteLenField.GetValue(reader);
            int charPos = (int)charPosField.GetValue(reader);
            if (charPos > 0)
            {
                position += reader.CurrentEncoding.GetBytes((char[])charBufferField.GetValue(reader), 0, charPos).Length;
            }
            return position;
        }

        /// <summary>
        /// Sets the position of the internal <see cref="Stream"/>.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <returns>The new position within the current stream.</returns>
        public static long Seek(this StreamReader reader, long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            reader.DiscardBufferedData();
            return reader.BaseStream.Seek(offset, origin);
        }
    }
}
