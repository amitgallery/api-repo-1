using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubDemo
{
    public class Folder
    {
        public string Name { get; set; }

        public List<Folder> SubFolders { get; set; }

        public List<FileInfo> Files { get; set; }

        public static Folder ToFolder(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            var folder = ConvertToFolder(di);
            folder.Name = string.Empty;

            return folder;
        }

        private static Folder ConvertToFolder(DirectoryInfo dirInfo)
        {
            var folder = new Folder();
            folder.Name = dirInfo.Name;

            var subFolders = dirInfo.GetDirectories();
            if(subFolders != null)
            {
                folder.SubFolders = new List<Folder>();
                foreach(var subFolder in subFolders)
                {
                    folder.SubFolders.Add(ConvertToFolder(subFolder));
                }
            }

            var files = dirInfo.GetFiles();
            if(files != null)
            {
                folder.Files = new List<FileInfo>();
                foreach(var file in files)
                {
                    var fileInfo = new FileInfo();
                    fileInfo.Name = file.Name;
                    using (var sr = file.OpenText())
                    {
                        fileInfo.Content = sr.ReadToEnd();
                    }

                    folder.Files.Add(fileInfo);
                }
            }

            return folder;
        }
    }
}
