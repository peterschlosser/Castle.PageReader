// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>
    /// Implements a TextReader optimized to reading lines backward from
    /// the end of a file <see cref="Stream"/>. The class is a clone of
    /// <see cref="StreamReader"/> and reads line input using the
    /// <see cref="Encoding"/> of the file.
    /// </summary>
    /// <remarks>
    /// This class is a clone of System.IO.StreamReader ASP.NET Core 2.1
    /// but optimized for reading lines from the end of the file backward
    /// and ideally suited to reading log text files, as they generally are
    /// read from the last line backward.  The best methods for doing so
    /// are ReadLine() and ReadLineAsync().  Read() and other methods of 
    /// this reverse <see cref="StreamReader"/> class become, more or less,
    /// useless in the reverse-reading context and defunct methods throw
    /// <see cref="NotSupportedException"/>.  Position() and Seek() methods
    /// provide random access to known lines in the file <see cref="Stream"/>.
    /// </remarks>
    // This class implements a TextReader for reading characters to a Stream.
    // This is designed for character input in a particular Encoding, 
    // whereas the Stream class is designed for byte input and output.  
    public class StreamReverseReader : TextReader
    {
        // StreamReader.Null is threadsafe.
        public new static readonly StreamReverseReader Null = new NullStreamReader();

        // Using a 1K byte buffer and a 4K FileStream buffer works out pretty well
        // perf-wise.  On even a 40 MB text file, any perf loss by using a 4K
        // buffer is negated by the win of allocating a smaller byte[], which 
        // saves construction time.  This does break adaptive buffering,
        // but this is slightly faster.
        private const int DefaultBufferSize = 1024;  // Byte buffer size
        private const int DefaultFileStreamBufferSize = 4096;
        private const int MinBufferSize = 128;

        private Stream _stream;
        private Encoding _encoding;
        private Decoder _decoder;
        private byte[] _byteBuffer;
        private char[] _charBuffer;
        private int _charPos;
        private int _charLen;
        // Record the number of valid bytes in the byteBuffer, for a few checks.
        private int _byteLen;
        // This is used only for preamble detection
        private int _bytePos;

        // This is the maximum number of chars we can get from one call to 
        // ReadBuffer.  Used so ReadBuffer can tell when to copy data into
        // a user's char[] directly, instead of our internal char[].
        private int _maxCharsPerBuffer;

        // We will support looking for byte order marks in the stream and trying
        // to decide what the encoding might be from the byte order marks, IF they
        // exist.  But that's all we'll do.  
        private bool _detectEncoding;

        // Whether we must still check for the encoding's given preamble at the
        // beginning of this file.
        private bool _checkPreamble;

        // Whether the stream is most likely not going to give us back as much 
        // data as we want the next time we call it.  We must do the computation
        // before we do any byte order mark handling and save the result.  Note
        // that we need this to allow users to handle streams used for an 
        // interactive protocol, where they block waiting for the remote end 
        // to send a response, like logging in on a Unix machine.
        private bool _isBlocked;

        // The intent of this field is to leave open the underlying stream when 
        // disposing of this StreamReader.  A name like _leaveOpen is better, 
        // but this type is serializable, and this field's name was _closable.
        private bool _closable;  // Whether to close the underlying stream.

        // We don't guarantee thread safety on StreamReader, but we should at 
        // least prevent users from trying to read anything while an Async
        // read from the same thread is in progress.
        private Task _asyncReadTask = Task.CompletedTask;

        private void CheckAsyncTaskInProgress()
        {
            // We are not locking the access to _asyncReadTask because this is not meant to guarantee thread safety. 
            // We are simply trying to deter calling any Read APIs while an async Read from the same thread is in progress.
            if (!_asyncReadTask.IsCompleted)
            {
                ThrowAsyncIOInProgress();
            }
        }

        private static void ThrowAsyncIOInProgress() =>
            throw new InvalidOperationException("The stream is currently in use by a previous operation on the stream.");

        // StreamReader by default will ignore illegal UTF8 characters. We don't want to 
        // throw here because we want to be able to read ill-formed data without choking. 
        // The high level goal is to be tolerant of encoding errors when we read and very strict 
        // when we write. Hence, default StreamWriter encoding will throw on error.   

        internal StreamReverseReader()
        {
        }

        public StreamReverseReader(Stream stream)
            : this(stream, true)
        {
        }

        public StreamReverseReader(Stream stream, bool detectEncodingFromByteOrderMarks)
            : this(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, DefaultBufferSize, false)
        {
        }

        public StreamReverseReader(Stream stream, Encoding encoding)
            : this(stream, encoding, true, DefaultBufferSize, false)
        {
        }

        public StreamReverseReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : this(stream, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize, false)
        {
        }

        // Creates a new StreamReader for the given stream.  The 
        // character encoding is set by encoding and the buffer size, 
        // in number of 16-bit characters, is set by bufferSize.  
        // 
        // Note that detectEncodingFromByteOrderMarks is a very
        // loose attempt at detecting the encoding by looking at the first
        // 3 bytes of the stream.  It will recognize UTF-8, little endian
        // unicode, and big endian unicode text, but that's it.  If neither
        // of those three match, it will use the Encoding you provided.
        // 
        public StreamReverseReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : this(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, false)
        {
        }

        public StreamReverseReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
        {
            if (stream == null || encoding == null)
            {
                throw new ArgumentNullException(stream == null ? nameof(stream) : nameof(encoding));
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream was not readable.");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Positive number required.");
            }

            Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen);
        }

        public StreamReverseReader(string path)
            : this(path, true)
        {
        }

        public StreamReverseReader(string path, bool detectEncodingFromByteOrderMarks)
            : this(path, Encoding.UTF8, detectEncodingFromByteOrderMarks, DefaultBufferSize)
        {
        }

        public StreamReverseReader(string path, Encoding encoding)
            : this(path, encoding, true, DefaultBufferSize)
        {
        }

        public StreamReverseReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : this(path, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize)
        {
        }

        public StreamReverseReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (path.Length == 0)
                throw new ArgumentException("Empty path name is not legal.");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Positive number required.");

            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                DefaultFileStreamBufferSize, FileOptions.SequentialScan);
            Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen: false);
        }

        private void Init(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
        {
            _stream = stream;
            _encoding = encoding;
            _decoder = encoding.GetDecoder();
            if (bufferSize < MinBufferSize)
            {
                bufferSize = MinBufferSize;
            }

            _byteBuffer = new byte[bufferSize];
            _maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            _charBuffer = new char[_maxCharsPerBuffer];
            _byteLen = 0;
            _bytePos = 0;
            _detectEncoding = detectEncodingFromByteOrderMarks;
            _checkPreamble = encoding.GetPreamble().Length > 0;
            _isBlocked = false;
            _closable = !leaveOpen;
            Seek(0, SeekOrigin.End);
        }

        // Init used by NullStreamReader, to delay load encoding
        internal void Init(Stream stream)
        {
            _stream = stream;
            _closable = true;
        }

        public override void Close()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            // Dispose of our resources if this StreamReader is closable.
            // Note that Console.In should be left open.
            try
            {
                // Note that Stream.Close() can potentially throw here. So we need to 
                // ensure cleaning up internal resources, inside the finally block.  
                if (!LeaveOpen && disposing && (_stream != null))
                {
                    _stream.Dispose();
                }
            }
            finally
            {
                if (!LeaveOpen && (_stream != null))
                {
                    _stream = null;
                    _encoding = null;
                    _decoder = null;
                    _byteBuffer = null;
                    _charBuffer = null;
                    _charPos = 0;
                    _charLen = 0;
                    base.Dispose(disposing);
                }
            }
        }

        public virtual Encoding CurrentEncoding
        {
            get { return _encoding; }
        }

        public virtual Stream BaseStream
        {
            get { return _stream; }
        }

        internal bool LeaveOpen
        {
            get { return !_closable; }
        }

        // DiscardBufferedData tells StreamReader to throw away its internal
        // buffer contents.  This is useful if the user needs to seek on the
        // underlying stream to a known location then wants the StreamReader
        // to start reading from this new point.  This method should be called
        // very sparingly, if ever, since it can lead to very poor performance.
        // However, it may be the only way of handling some scenarios where 
        // users need to re-read the contents of a StreamReader a second time.
        public void DiscardBufferedData()
        {
            CheckAsyncTaskInProgress();

            _byteLen = 0;
            _charLen = 0;
            _charPos = 0;
            // in general we'd like to have an invariant that encoding isn't null. However,
            // for startup improvements for NullStreamReader, we want to delay load encoding. 
            if (_encoding != null)
            {
                _decoder = _encoding.GetDecoder();
            }
            _isBlocked = false;
        }

        public bool EndOfStream
        {
            get
            {
                if (_stream == null)
                {
                    throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
                }

                CheckAsyncTaskInProgress();

                if (_charPos < _charLen)
                {
                    return false;
                }

                // This may block on pipes!
                int numRead = ReadBuffer();
                return numRead == 0;
            }
        }

        public override int Peek()
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        /// <summary>
        /// Gets the position of the internal <see cref="Stream"/>.
        /// </summary>
        /// <returns>The position in the stream.</returns>
        public virtual long Position()
        {
            return BaseStream.Position + (_charPos > 0 ? CurrentEncoding.GetBytes(_charBuffer, 0, _charPos + 1).Length : 0);
        }

        public override int Read()
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        public override int Read(char[] buffer, int index, int count)
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        public override string ReadToEnd()
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        /// <summary>
        /// Sets the position of the internal <see cref="Stream"/>.
        /// </summary>
        /// <param name="offset">he point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for offset, using a value of type <see cref="SeekOrigin"/>.</param>
        /// <returns>The new position in the stream.</returns>
        public virtual long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            _checkPreamble = origin == SeekOrigin.Begin && offset == 0;
            DiscardBufferedData();
            return BaseStream.Seek(offset, origin);
        }

        private void CompressBuffer(int n)
        {
            Debug.Assert(_byteLen >= n, "CompressBuffer was called with a number of bytes greater than the current buffer length.  Are two threads using this StreamReader at the same time?");
            Buffer.BlockCopy(_byteBuffer, n, _byteBuffer, 0, _byteLen - n);
            _byteLen -= n;
        }

        private void DetectEncoding()
        {
            if (_byteLen < 2)
            {
                return;
            }
            _detectEncoding = false;
            bool changedEncoding = false;
            if (_byteBuffer[0] == 0xFE && _byteBuffer[1] == 0xFF)
            {
                // Big Endian Unicode

                _encoding = Encoding.BigEndianUnicode;
                CompressBuffer(2);
                changedEncoding = true;
            }

            else if (_byteBuffer[0] == 0xFF && _byteBuffer[1] == 0xFE)
            {
                // Little Endian Unicode, or possibly little endian UTF32
                if (_byteLen < 4 || _byteBuffer[2] != 0 || _byteBuffer[3] != 0)
                {
                    _encoding = Encoding.Unicode;
                    CompressBuffer(2);
                    changedEncoding = true;
                }
                else
                {
                    _encoding = Encoding.UTF32;
                    CompressBuffer(4);
                    changedEncoding = true;
                }
            }

            else if (_byteLen >= 3 && _byteBuffer[0] == 0xEF && _byteBuffer[1] == 0xBB && _byteBuffer[2] == 0xBF)
            {
                // UTF-8
                _encoding = Encoding.UTF8;
                CompressBuffer(3);
                changedEncoding = true;
            }
            else if (_byteLen >= 4 && _byteBuffer[0] == 0 && _byteBuffer[1] == 0 &&
                _byteBuffer[2] == 0xFE && _byteBuffer[3] == 0xFF)
            {
                // Big Endian UTF32
                _encoding = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
                CompressBuffer(4);
                changedEncoding = true;
            }
            else if (_byteLen == 2)
            {
                _detectEncoding = true;
            }
            // Note: in the future, if we change this algorithm significantly,
            // we can support checking for the preamble of the given encoding.

            if (changedEncoding)
            {
                _decoder = _encoding.GetDecoder();
                int newMaxCharsPerBuffer = _encoding.GetMaxCharCount(_byteBuffer.Length);
                if (newMaxCharsPerBuffer > _maxCharsPerBuffer)
                {
                    _charBuffer = new char[newMaxCharsPerBuffer];
                }
                _maxCharsPerBuffer = newMaxCharsPerBuffer;
            }
        }

        // Trims the preamble bytes from the byteBuffer. This routine can be called multiple times
        // and we will buffer the bytes read until the preamble is matched or we determine that
        // there is no match. If there is no match, every byte read previously will be available 
        // for further consumption. If there is a match, we will compress the buffer for the 
        // leading preamble bytes
        private bool IsPreamble()
        {
            if (!_checkPreamble)
            {
                return _checkPreamble;
            }

            var preamble = _encoding.GetPreamble();

            Debug.Assert(_bytePos <= preamble.Length, "_compressPreamble was called with the current bytePos greater than the preamble buffer length.  Are two threads using this StreamReader at the same time?");
            int len = (_byteLen >= (preamble.Length)) ? (preamble.Length - _bytePos) : (_byteLen - _bytePos);

            for (int i = 0; i < len; i++, _bytePos++)
            {
                if (_byteBuffer[_bytePos] != preamble[_bytePos])
                {
                    _bytePos = 0;
                    _checkPreamble = false;
                    break;
                }
            }

            Debug.Assert(_bytePos <= preamble.Length, "possible bug in _compressPreamble.  Are two threads using this StreamReader at the same time?");

            if (_checkPreamble)
            {
                if (_bytePos == preamble.Length)
                {
                    // We have a match
                    CompressBuffer(preamble.Length);
                    _bytePos = 0;
                    _checkPreamble = false;
                    _detectEncoding = false;
                }
            }

            return _checkPreamble;
        }

        /// <summary>
        /// Reads data from the internal file <see cref="Stream"/> into the internal buffer.
        /// </summary>
        /// <returns>A count of the characters contained in the internal buffer using the file 
        /// character <see cref="Encoding"/></returns>
        /// <remarks>It is presumed on the first call, we are reading from Start of File (SOF) so
        /// the Preamble is tested and when finding a match, buffer pointers adjusted so the 
        /// Preamble isn't treated as read data.</remarks>
        internal virtual int ReadBuffer()
        {
            _charLen = 0;
            _charPos = 0;

            if (!_checkPreamble)
                _byteLen = 0;
            do
            {
                if (_checkPreamble)
                {
                    Debug.Assert(_bytePos <= _encoding.GetPreamble().Length, "possible bug in _compressPreamble.  Are two threads using this StreamReader at the same time?");
                    int len = _stream.Read(_byteBuffer, _bytePos, _byteBuffer.Length - _bytePos);
                    Debug.Assert(len >= 0, "Stream.Read returned a negative number!  This is a bug in your stream class.");

                    if (len == 0)
                    {
                        // EOF but we might have buffered bytes from previous 
                        // attempt to detect preamble that needs to be decoded now
                        if (_byteLen > 0)
                        {
                            _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
                            // Need to zero out the byteLen after we consume these bytes so that we don't keep infinitely hitting this code path
                            _bytePos = _byteLen = 0;
                        }

                        return _charLen;
                    }

                    _byteLen += len;
                }
                else
                {
                    long readLen = Math.Min(_stream.Position, _byteBuffer.Length);

                    if (readLen == 0)  // We're at SOF
                        return _charLen;

                    if (BaseStream.Seek(-readLen, SeekOrigin.Current) == 0)
                        _detectEncoding = true;

                    Debug.Assert(_bytePos == 0, "bytePos can be non zero only when we are trying to _checkPreamble.  Are two threads using this StreamReader at the same time?");
                    _byteLen = _stream.Read(_byteBuffer, 0, (int)readLen);
                    Debug.Assert(_byteLen >= 0, "Stream.Read returned a negative number!  This is a bug in your stream class.");

                    BaseStream.Seek(-readLen, SeekOrigin.Current);

                    if (_byteLen == 0)  // We're at EOF
                        return _charLen;
                }

                // _isBlocked == whether we read fewer bytes than we asked for.
                // Note we must check it here because CompressBuffer or 
                // DetectEncoding will change byteLen.
                _isBlocked = (_byteLen < _byteBuffer.Length);

                // Check for preamble before detect encoding. This is not to override the
                // user supplied Encoding for the one we implicitly detect. The user could
                // customize the encoding which we will loose, such as ThrowOnError on UTF8
                if (IsPreamble())
                {
                    continue;
                }

                // If we're supposed to detect the encoding and haven't done so yet,
                // do it.  Note this may need to be called more than once.
                if (_detectEncoding && _byteLen >= 2)
                {
                    DetectEncoding();
                }

                _charPos = _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
                _charPos -= _charPos > 0 ? 1 : 0;
            } while (_charLen == 0);
            //Console.WriteLine("ReadBuffer called.  chars: "+charLen);
            return _charLen;
        }

        private int ReadBuffer(char[] userBuffer, int userOffset, int desiredChars, out bool readToUserBuffer)
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        // Reads a line. A line is defined as a sequence of characters followed by
        // a carriage return ('\r'), a line feed ('\n'), or a carriage return
        // immediately followed by a line feed. The resulting string does not
        // contain the terminating carriage return and/or line feed. The returned
        // value is null if the end of the input stream has been reached.
        //
        public override string ReadLine()
        {
            if (_stream == null)
            {
                throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
            }

            CheckAsyncTaskInProgress();

            if (_charPos == 0 && ReadBuffer() == 0)
            {
                return null;
            }

            StringBuilder sb = null;
            bool crlf = false;
            do
            {
                int i = _charPos;
                do
                {
                    char ch = _charBuffer[i];
                    // Note the following common line feed chars:
                    // \n - UNIX   \r\n - DOS   \r - Mac
                    if (ch == '\r' || ch == '\n')
                    {
                        if (crlf)
                        {
                            string s = null;
                            if (sb != null)
                            {
                                sb.Insert(0, _charBuffer, i + 1, _charPos - i);
                                s = sb.ToString();
                            }
                            else
                            {
                                s = new string(_charBuffer, i + 1, _charPos - i);
                            }
                            _charPos = i;
                            return s;
                        }
                        _charPos = i - 1;
                        if (ch == '\n' && (_charPos >= 0 || ReadBuffer() > 0))
                        {
                            // \r\n - DOS
                            if (_charBuffer[_charPos] == '\r')
                            {
                                i--;
                                _charPos = i - 1;
                            }
                        }
                        crlf = true;
                    }
                    i--;
                } while (i > 0);
                if (sb == null)
                {
                    sb = new StringBuilder(_charPos + 80);
                }
                sb.Insert(0, _charBuffer, 0, _charPos + 1);
            } while (ReadBuffer() > 0);
            return sb.ToString();
        }

        #region Task based Async APIs
        public override Task<string> ReadLineAsync()
        {
            // If we have been inherited into a subclass, the following implementation could be incorrect
            // since it does not call through to Read() which a subclass might have overridden.  
            // To be safe we will only use this implementation in cases where we know it is safe to do so,
            // and delegate to our base class (which will call into Read) when we are not sure.
            if (GetType() != typeof(StreamReverseReader))
            {
                return base.ReadLineAsync();
            }

            if (_stream == null)
            {
                throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
            }

            CheckAsyncTaskInProgress();

            Task<string> task = ReadLineAsyncInternal();
            _asyncReadTask = task;

            return task;
        }

        private async Task<string> ReadLineAsyncInternal()
        {
            if (_charPos == 0 && (await ReadBufferAsync().ConfigureAwait(false)) == 0)
            {
                return null;
            }

            StringBuilder sb = null;
            bool crlf = false;

            do
            {
                char[] tmpCharBuffer = _charBuffer;
                int tmpCharLen = _charLen;
                int tmpCharPos = _charPos;
                int i = tmpCharPos;

                do
                {
                    char ch = tmpCharBuffer[i];

                    // Note the following common line feed chars:
                    // \n - UNIX   \r\n - DOS   \r - Mac
                    if (ch == '\r' || ch == '\n')
                    {
                        if (crlf)
                        {
                            string s = null;
                            if (sb != null)
                            {
                                sb.Insert(0, tmpCharBuffer, i + 1, tmpCharPos - i);
                                s = sb.ToString();
                            }
                            else
                            {
                                s = new string(tmpCharBuffer, i + 1, tmpCharPos - i);
                            }
                            _charPos = i;
                            return s;
                        }
                        _charPos = tmpCharPos = i - 1;
                        if (ch == '\n' && (tmpCharPos >= 0 || (await ReadBufferAsync().ConfigureAwait(false)) > 0))
                        {
                            // \r\n - DOS
                            tmpCharPos = _charPos;
                            if (tmpCharBuffer[tmpCharPos] == '\r')
                            {
                                i--;
                                _charPos = tmpCharPos = i - 1;
                            }
                        }
                        crlf = true;
                    }
                    i--;
                } while (i > 0);
                if (sb == null)
                {
                    sb = new StringBuilder(tmpCharPos + 80);
                }
                sb.Insert(0, tmpCharBuffer, 0, tmpCharPos + 1);
            } while (await ReadBufferAsync().ConfigureAwait(false) > 0);

            return sb.ToString();
        }

        public override Task<string> ReadToEndAsync()
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        private Task<string> ReadToEndAsyncInternal()
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
        {
            throw new NotSupportedException("Specified method is not supported.");
        }

        private async Task<int> ReadBufferAsync()
        {
            _charLen = 0;
            _charPos = 0;
            Byte[] tmpByteBuffer = _byteBuffer;
            Stream tmpStream = _stream;

            if (!_checkPreamble)
            {
                _byteLen = 0;
            }
            do
            {
                if (_checkPreamble)
                {
                    Debug.Assert(_bytePos <= _encoding.GetPreamble().Length, "possible bug in _compressPreamble. Are two threads using this StreamReader at the same time?");
                    int tmpBytePos = _bytePos;
                    int len = await tmpStream.ReadAsync(tmpByteBuffer, tmpBytePos, tmpByteBuffer.Length - tmpBytePos).ConfigureAwait(false);
                    Debug.Assert(len >= 0, "Stream.Read returned a negative number!  This is a bug in your stream class.");

                    if (len == 0)
                    {
                        // EOF but we might have buffered bytes from previous 
                        // attempt to detect preamble that needs to be decoded now
                        if (_byteLen > 0)
                        {
                            _charLen += _decoder.GetChars(tmpByteBuffer, 0, _byteLen, _charBuffer, _charLen);
                            // Need to zero out the _byteLen after we consume these bytes so that we don't keep infinitely hitting this code path
                            _bytePos = 0; _byteLen = 0;
                        }

                        return _charLen;
                    }

                    _byteLen += len;
                }
                else
                {
                    long readLen = Math.Min(_stream.Position, tmpByteBuffer.Length);

                    if (readLen == 0)  // We're at SOF
                        return _charLen;

                    if (BaseStream.Seek(-readLen, SeekOrigin.Current) == 0)
                        _detectEncoding = true;

                    Debug.Assert(_bytePos == 0, "bytePos can be non zero only when we are trying to _checkPreamble.  Are two threads using this StreamReader at the same time?");
                    _byteLen = await tmpStream.ReadAsync(tmpByteBuffer, 0, (int)readLen).ConfigureAwait(false);
                    Debug.Assert(_byteLen >= 0, "Stream.Read returned a negative number!  Bug in stream class.");

                    BaseStream.Seek(-readLen, SeekOrigin.Current);

                    if (_byteLen == 0)  // We're at EOF
                        return _charLen;
                }

                // _isBlocked == whether we read fewer bytes than we asked for.
                // Note we must check it here because CompressBuffer or 
                // DetectEncoding will change _byteLen.
                _isBlocked = (_byteLen < tmpByteBuffer.Length);

                // Check for preamble before detect encoding. This is not to override the
                // user supplied Encoding for the one we implicitly detect. The user could
                // customize the encoding which we will loose, such as ThrowOnError on UTF8
                if (IsPreamble())
                {
                    continue;
                }

                // If we're supposed to detect the encoding and haven't done so yet,
                // do it.  Note this may need to be called more than once.
                if (_detectEncoding && _byteLen >= 2)
                {
                    DetectEncoding();
                }

                _charPos = _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
                _charPos -= _charPos > 0 ? 1 : 0;
            } while (_charLen == 0);

            return _charLen;
        }
        #endregion

        // No data, class doesn't need to be serializable.
        // Note this class is threadsafe.
        private class NullStreamReader : StreamReverseReader
        {
            // Instantiating Encoding causes unnecessary perf hit. 
            internal NullStreamReader()
            {
                Init(Stream.Null);
            }

            public override Stream BaseStream
            {
                get { return Stream.Null; }
            }

            public override Encoding CurrentEncoding
            {
                get { return Encoding.Unicode; }
            }

            protected override void Dispose(bool disposing)
            {
                // Do nothing - this is essentially unclosable.
            }

            public override int Peek()
            {
                return -1;
            }

            public override int Read()
            {
                return -1;
            }

            [SuppressMessage("Microsoft.Contracts", "CC1055")]  // Skip extra error checking to avoid *potential* AppCompat problems.
            public override int Read(char[] buffer, int index, int count)
            {
                return 0;
            }

            public override string ReadLine()
            {
                return null;
            }

            public override string ReadToEnd()
            {
                return string.Empty;
            }

            internal override int ReadBuffer()
            {
                return 0;
            }
        }
    }
}
