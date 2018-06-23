using System;

namespace LogFileReader.Models
{
    public class LogFileListModel : Castle.PageReader.Models.PageReaderFile
    {
        public LogFileListModel(Castle.PageReader.Models.PageReaderFile file)
        {
            Name = file.Name;
            Length = Math.Round(file.Length / 1024D).ToString("N0") + " KB";
        }

        public new string Length { get; protected set; }
    }
}
