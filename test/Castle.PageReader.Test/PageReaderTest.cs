using Castle.PageReader.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Castle.PageReader.Test
{
    public class PageReaderTest : IClassFixture<PageReaderFixture>
    {
        private readonly PageReaderFixture Fixture;

        public PageReaderTest(PageReaderFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public void ListsFiles()
        {
            var fileList = PageReader.GetFiles();

            Assert.Single(fileList);

            Assert.Equal(Fixture.TestName, fileList.First().Name);
            Assert.Equal(Fixture.FileLength, fileList.First().Length);
        }

        [Fact]
        public async Task ListsFilesAsync()
        {
            var fileList = await PageReader.GetFilesAsync();

            Assert.Single(fileList);

            Assert.Equal(Fixture.TestName, fileList.First().Name);
            Assert.Equal(Fixture.FileLength, fileList.First().Length);
        }

        [Fact]
        public void ForwardReadsFirst()
        {
            var count = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = count
            };
            long pageTop = PageReaderData.EOF;
            long pageBottom = count * Fixture.TextLine(0).Length;

            Assert.True(Fixture.LineCount > logReader.Count, "Not enough lines to perform test.");

            PageReader.ReadFirst(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(count, logReader.Lines.Count());
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(Fixture.Text(i + 1), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public async Task ForwardReadsFirstAsync()
        {
            var count = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = count
            };
            long pageTop = PageReaderData.EOF;
            long pageBottom = count * Fixture.TextLine(0).Length;

            Assert.True(Fixture.LineCount > logReader.Count, "Not enough lines to perform test.");

            await PageReader.ReadFirstAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(count, logReader.Lines.Count());
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(Fixture.Text(i + 1), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public void ForwardReadsLast()
        {
            var count = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = count
            };
            long pageTop = (Fixture.LineCount - count) * Fixture.TextLine(0).Length;
            long pageBottom = PageReaderData.EOF;

            Assert.True(Fixture.LineCount > logReader.Count, "Not enough lines to perform test.");

            PageReader.ReadLast(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(count, logReader.Lines.Count());
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(Fixture.Text(Fixture.LineCount - count + i + 1), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public async Task ForwardReadsLastAsync()
        {
            var count = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = count
            };
            long pageTop = (Fixture.LineCount - count) * Fixture.TextLine(0).Length;
            long pageBottom = PageReaderData.EOF;

            Assert.True(Fixture.LineCount > logReader.Count, "Not enough lines to perform test.");

            await PageReader.ReadLastAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(count, logReader.Lines.Count());
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(Fixture.Text(Fixture.LineCount - count + i + 1), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public void ForwardReadsNext()
        {
            var firstPageCount = 10;
            var nextPageCount = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = firstPageCount
            };
            long pageTop = PageReaderData.EOF;
            long pageBottom = (firstPageCount) * Fixture.TextLine(0).Length;

            Assert.True(Fixture.LineCount > firstPageCount + nextPageCount, "Not enough lines to perform test.");

            PageReader.ReadFirst(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(firstPageCount, logReader.Lines.Count());

            pageTop = pageBottom;
            pageBottom = (firstPageCount + nextPageCount) * Fixture.TextLine(0).Length;
            logReader.Count = nextPageCount;

            PageReader.ReadNext(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(nextPageCount, logReader.Lines.Count());
            for (var i = 0; i < nextPageCount; i++)
            {
                Assert.Equal(Fixture.Text(firstPageCount + i + 1), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public async Task ForwardReadsNextAsync()
        {
            var firstPageCount = 10;
            var nextPageCount = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = firstPageCount
            };
            long pageTop = PageReaderData.EOF;
            long pageBottom = (firstPageCount) * Fixture.TextLine(0).Length;

            Assert.True(Fixture.LineCount > firstPageCount + nextPageCount, "Not enough lines to perform test.");

            await PageReader.ReadFirstAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(firstPageCount, logReader.Lines.Count());

            pageTop = pageBottom;
            pageBottom = (firstPageCount + nextPageCount) * Fixture.TextLine(0).Length;
            logReader.Count = nextPageCount;

            await PageReader.ReadNextAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(nextPageCount, logReader.Lines.Count());
            for (var i = 0; i < nextPageCount; i++)
            {
                Assert.Equal(Fixture.Text(firstPageCount + i + 1), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public void ForwardReadsPrev()
        {
            var lastPageCount = 10;
            var prevPageCount = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = lastPageCount
            };
            long pageTop = (Fixture.LineCount - lastPageCount) * Fixture.TextLine(0).Length;
            long pageBottom = PageReaderData.EOF;

            Assert.True(Fixture.LineCount > lastPageCount, "Not enough lines to perform test.");

            PageReader.ReadLast(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(lastPageCount, logReader.Lines.Count());

            pageBottom = pageTop;
            pageTop = (Fixture.LineCount - lastPageCount - prevPageCount) * Fixture.TextLine(0).Length;
            logReader.Count = prevPageCount;

            PageReader.ReadPrev(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(prevPageCount, logReader.Lines.Count());
            for (var i = 0; i < prevPageCount; i++)
            {
                Assert.Equal(Fixture.Text(lastPageCount + prevPageCount + i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public async Task ForwardReadsPrevAsync()
        {
            var lastPageCount = 10;
            var prevPageCount = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = lastPageCount
            };
            long pageTop = (Fixture.LineCount - lastPageCount) * Fixture.TextLine(0).Length;
            long pageBottom = PageReaderData.EOF;

            Assert.True(Fixture.LineCount > lastPageCount, "Not enough lines to perform test.");

            await PageReader.ReadLastAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(lastPageCount, logReader.Lines.Count());

            pageBottom = pageTop;
            pageTop = (Fixture.LineCount - lastPageCount - prevPageCount) * Fixture.TextLine(0).Length;
            logReader.Count = prevPageCount;

            await PageReader.ReadPrevAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(prevPageCount, logReader.Lines.Count());
            for (var i = 0; i < prevPageCount; i++)
            {
                Assert.Equal(Fixture.Text(lastPageCount + prevPageCount + i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public void ReverseReadsFirst()
        {
            var count = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = count
            };
            long pageTop = PageReaderData.EOF;
            long pageBottom = (Fixture.LineCount - logReader.Count) * Fixture.TextLine(0).Length;

            Assert.True(Fixture.LineCount > logReader.Count, "Not enough lines to perform test.");

            LogPageReader.ReadFirst(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(count, logReader.Lines.Count());
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(Fixture.Text(Fixture.LineCount - i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public async Task ReverseReadsFirstAsync()
        {
            var count = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = count
            };
            long pageTop = PageReaderData.EOF;
            long pageBottom = (Fixture.LineCount - logReader.Count) * Fixture.TextLine(0).Length;

            Assert.True(Fixture.LineCount > logReader.Count, "Not enough lines to perform test.");

            await LogPageReader.ReadFirstAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(count, logReader.Lines.Count());
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(Fixture.Text(Fixture.LineCount - i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public void ReverseReadsLast()
        {
            var count = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = count
            };
            long pageTop = (logReader.Count) * Fixture.TextLine(0).Length;
            long pageBottom = PageReaderData.EOF;

            Assert.True(Fixture.LineCount > logReader.Count, "Not enough lines to perform test.");

            LogPageReader.ReadLast(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(count, logReader.Lines.Count());
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(Fixture.Text(count - i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public async Task ReverseReadsLastAsync()
        {
            var count = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = count
            };
            long pageTop = (logReader.Count) * Fixture.TextLine(0).Length;
            long pageBottom = PageReaderData.EOF;

            Assert.True(Fixture.LineCount > logReader.Count, "Not enough lines to perform test.");

            await LogPageReader.ReadLastAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(count, logReader.Lines.Count());
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(Fixture.Text(count - i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public void ReverseReadsNext()
        {
            var firstPageCount = 10;
            var nextPageCount = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = firstPageCount
            };
            long pageTop = PageReaderData.EOF;
            long pageBottom = (Fixture.LineCount - logReader.Count) * Fixture.TextLine(0).Length;

            Assert.True(Fixture.LineCount > firstPageCount + nextPageCount, "Not enough lines to perform test.");

            LogPageReader.ReadFirst(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(firstPageCount, logReader.Lines.Count());

            pageTop = pageBottom;
            pageBottom = (Fixture.LineCount - firstPageCount - nextPageCount) * Fixture.TextLine(0).Length;
            logReader.Count = nextPageCount;

            LogPageReader.ReadNext(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(nextPageCount, logReader.Lines.Count());
            for (var i = 0; i < nextPageCount; i++)
            {
                Assert.Equal(Fixture.Text(Fixture.LineCount - firstPageCount - i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public async Task ReverseReadsNextAsync()
        {
            var firstPageCount = 10;
            var nextPageCount = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = firstPageCount
            };
            long pageTop = PageReaderData.EOF;
            long pageBottom = (Fixture.LineCount - logReader.Count) * Fixture.TextLine(0).Length;

            Assert.True(Fixture.LineCount > firstPageCount + nextPageCount, "Not enough lines to perform test.");

            await LogPageReader.ReadFirstAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(firstPageCount, logReader.Lines.Count());

            pageTop = pageBottom;
            pageBottom = (Fixture.LineCount - firstPageCount - nextPageCount) * Fixture.TextLine(0).Length;
            logReader.Count = nextPageCount;

            await LogPageReader.ReadNextAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(nextPageCount, logReader.Lines.Count());
            for (var i = 0; i < nextPageCount; i++)
            {
                Assert.Equal(Fixture.Text(Fixture.LineCount - firstPageCount - i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public void ReverseReadsPrev()
        {
            var lastPageCount = 10;
            var prevPageCount = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = lastPageCount
            };
            long pageTop = (lastPageCount) * Fixture.TextLine(0).Length;
            long pageBottom = PageReaderData.EOF;

            Assert.True(Fixture.LineCount > lastPageCount, "Not enough lines to perform test.");

            LogPageReader.ReadLast(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(lastPageCount, logReader.Lines.Count());

            pageBottom = pageTop;
            pageTop = (lastPageCount + prevPageCount) * Fixture.TextLine(0).Length;
            logReader.Count = prevPageCount;

            LogPageReader.ReadPrev(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(prevPageCount, logReader.Lines.Count());
            for (var i = 0; i < prevPageCount; i++)
            {
                Assert.Equal(Fixture.Text(lastPageCount + prevPageCount - i), logReader.Lines.Skip(i).First());
            }
        }

        [Fact]
        public async Task ReverseReadsPrevAsync()
        {
            var lastPageCount = 10;
            var prevPageCount = 3;
            var logReader = new PageReaderData()
            {
                Id = Fixture.TestName,
                Count = lastPageCount
            };
            long pageTop = (lastPageCount) * Fixture.TextLine(0).Length;
            long pageBottom = PageReaderData.EOF;

            Assert.True(Fixture.LineCount > lastPageCount, "Not enough lines to perform test.");

            await LogPageReader.ReadLastAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(lastPageCount, logReader.Lines.Count());

            pageBottom = pageTop;
            pageTop = (lastPageCount + prevPageCount) * Fixture.TextLine(0).Length;
            logReader.Count = prevPageCount;

            await LogPageReader.ReadPrevAsync(logReader);

            Assert.Equal(pageTop, logReader.PageTop);
            Assert.Equal(pageBottom, logReader.PageBottom);
            Assert.Equal(prevPageCount, logReader.Lines.Count());
            for (var i = 0; i < prevPageCount; i++)
            {
                Assert.Equal(Fixture.Text(lastPageCount + prevPageCount - i), logReader.Lines.Skip(i).First());
            }
        }
    }
}
