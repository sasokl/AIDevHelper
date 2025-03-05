using AIDevHelper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AIDevHelper.Services
{
    public class JsonHandler
    {
        public ProjectChanges ParseProjectChanges(string json)
        {
            // Попытка десериализации стандартного формата изменений
            var changes = JsonConvert.DeserializeObject<ProjectChanges>(json);
            if (changes != null &&
                ((changes.Delete != null && changes.Delete.Count > 0) ||
                 (changes.Modify != null && changes.Modify.Count > 0) ||
                 (changes.Create != null && changes.Create.Count > 0)))
            {
                return changes;
            }

            // Если стандартный формат не обнаружен, пытаемся обработать формат New Project
            try
            {
                var jObj = JObject.Parse(json);
                if (jObj["folders"] != null && jObj["files"] != null)
                {
                    var newProj = jObj.ToObject<NewProjectResponse>();
                    var projChanges = new ProjectChanges();
                    if (newProj.Files != null)
                    {
                        foreach (var file in newProj.Files)
                        {
                            projChanges.Create.Add(new FileItem { Path = file.Path, Code = file.Code });
                        }
                    }
                    return projChanges;
                }
            }
            catch
            {
                // Если не удалось распознать формат, возвращаем пустые изменения
            }

            return new ProjectChanges();
        }

        public ProjectStructure ParseProjectStructure(string json)
        {
            return JsonConvert.DeserializeObject<ProjectStructure>(json)
                   ?? new ProjectStructure();
        }
    }

    public class NewProjectResponse
    {
        public List<string> Folders { get; set; }
        public List<FileItem> Files { get; set; }

        public NewProjectResponse()
        {
            Folders = new List<string>();
            Files = new List<FileItem>();
        }
    }
}
