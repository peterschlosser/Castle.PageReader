namespace LogFileReader.Models
{
    public class LogFileDataModel : Castle.PageReader.Models.PageReaderData
    {
        public LogFileDataModel(Castle.PageReader.Models.PageReaderData page)
        {
            Id = page.Id;
            Count = page.Count;
            PageTop = page.PageTop;
            PageBottom = page.PageBottom;
            Lines = page.Lines;
        }
    }
}
