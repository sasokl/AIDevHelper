// ===============================================
// Updated ProjectJsonModel.cs with partial-modification support
// ===============================================
using System.Collections.Generic;

namespace AIDevHelper.Models
{
    public class ProjectStructure
    {
        public List<string> Folders { get; set; }
        public List<FileItem> Files { get; set; }

        public ProjectStructure()
        {
            Folders = new List<string>();
            Files = new List<FileItem>();
        }
    }

    public class FileItem
    {
        public string Path { get; set; }
        public string Code { get; set; }
        // New fields for partial modifications:
        public int? LineStart { get; set; }
        public int? LineEnd { get; set; }

        public FileItem()
        {
            Path = string.Empty;
            Code = string.Empty;
        }
    }

    public class ProjectChanges
    {
        public List<string> Delete { get; set; }
        public List<FileItem> Modify { get; set; }
        public List<FileItem> Create { get; set; }

        public ProjectChanges()
        {
            Delete = new List<string>();
            Modify = new List<FileItem>();
            Create = new List<FileItem>();
        }
    }
}
