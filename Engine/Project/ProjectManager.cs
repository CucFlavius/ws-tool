using ProjectWS.Engine.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ProjectWS.Engine.Project
{
    public class ProjectManager
    {
        public static Project? project;
        public static string? projectPath;

        public const string PROJECT_EXTENSION = "wsProject";

        public static void LoadProject(string? path)
        {
            if (path == null || path == String.Empty)
                return;

            if (File.Exists(path))
            {
                string? jsonString = File.ReadAllText(path);
                if (jsonString != null && jsonString != String.Empty)
                {
                    project = JsonSerializer.Deserialize<Project>(jsonString);
                    projectPath = path;
                    Engine.settings.projectManager.previousLoadedProject = path;
                    SettingsSerializer.Save();
                    Debug.Log("Loaded Project : " + project?.Name);
                }
            }
        }

        public static void CreateProject(string path)
        {
            project = new Project();
            project.Name = Path.GetFileNameWithoutExtension(path);
            project.UUID = Guid.NewGuid();
            projectPath = path;
            SaveProject();
            Engine.settings.projectManager.previousLoadedProject = path;
            SettingsSerializer.Save();

            Debug.Log("Created new Project : " + project.Name);
        }

        public static void SaveProject()
        {
            if (projectPath == null || projectPath == String.Empty)
                return;

            if (project == null)
                return;

            var options = new JsonSerializerOptions { WriteIndented = true };
            string data = JsonSerializer.Serialize(project, options);
            File.WriteAllText(projectPath, data);

            // TODO: Save all map changes here
        }
    }
}
