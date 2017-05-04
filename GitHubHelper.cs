using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubDemo
{
    public class GitHubHelper
    {
        private GitHubClient client;
        private Repository repo;

        public GitHubHelper()
        {
            var token = "0ed16dbfa7996251b394b8735ccdc9f12aa221ac";

            client = new GitHubClient(new ProductHeaderValue("amit-test-app"));

            var tokenAuth = new Credentials(token);
            client.Credentials = tokenAuth;

            var reponame = "api-repo-1";

            var organizationLogin = "amitgallery";

            var getRepo = client.Repository.Get(organizationLogin, reponame);
            getRepo.Wait();

            repo = getRepo.Result;
        }

        public void Upload(Folder folderInfo)
        {
            var tree = this.ConstructTree(folderInfo);
            var createdTree = client.Git.Tree.Create(repo.Id, tree);
            createdTree.Wait();


            // 4 - this commit should build on the current master branch
            var master = client.Git.Reference.Get(repo.Id, "heads/master");
            master.Wait();
            var newCommit = new NewCommit(
                "Hello World!",
                createdTree.Result.Sha,
              new[] { master.Result.Object.Sha })
            { Author = new Committer("ram", "ram@amit.com", DateTime.UtcNow) };



            var createdCommit = client.Git.Commit.Create(repo.Id, newCommit);
            createdCommit.Wait();

            // 5 - create a reference for the master branch

            var updateReference = new ReferenceUpdate(createdCommit.Result.Sha);

            var updatedReference = client.Git.Reference.Update(repo.Id, "heads/master", updateReference);
            updatedReference.Wait();
        }

        private NewTree ConstructTree(Folder folderInfo)
        {
            var tree = new NewTree();

            this.ConstructTreeInternal(folderInfo, tree, string.Empty);

            return tree;
        }

        private void ConstructTreeInternal(Folder folderInfo, NewTree tree, string parentPath)
        {
            var path = string.Empty;

            if(string.IsNullOrEmpty(folderInfo.Name))
            {
                if(!string.IsNullOrEmpty(parentPath))
                {
                    throw new ArgumentException("Invalid arguments");
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(parentPath))
                {
                    path = parentPath;
                }

                path = path + folderInfo.Name + "/";
            }

            if(folderInfo.Files != null)
            {
                foreach(var file in folderInfo.Files)
                {
                    var filePath = path + file.Name;

                    var newBlob = new NewBlob()
                    {

                        Content = file.Content,
                        Encoding = EncodingType.Utf8
                    };

                    var createdBlob = client.Git.Blob.Create(repo.Id, newBlob);
                    createdBlob.Wait();
                    var item = new NewTreeItem()
                    {

                        Path = filePath,
                        Mode = Octokit.FileMode.File,
                        Sha = createdBlob.Result.Sha,
                        Type = TreeType.Blob
                    };

                    tree.Tree.Add(item);
                }
            }

            if(folderInfo.SubFolders != null)
            {
                foreach(var folder in folderInfo.SubFolders)
                {
                    this.ConstructTreeInternal(folder, tree, path);
                }
            }
        }
    }
}
