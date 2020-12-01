using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageServer.Models
{
    public class SaveFile
    {
        [JsonPropertyName("Files")]
        public List<FileData> Files { get; set; }
        [JsonPropertyName("ProductID")]
        public int ProductID { get; set; }
    }

    public class FileData
    {
        [JsonPropertyName("Data")]
        public byte[] Data { get; set; }
        [JsonPropertyName("FileType")]
        public string FileType { get; set; }
        [JsonPropertyName("Size")]
        public long Size { get; set; }
    }
}
