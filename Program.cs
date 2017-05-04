using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var folderPath = "C:\\Users\\amitar\\Documents\\Visual Studio 2015\\Projects\\GitHubDemo\\GitHubDemo";

            var folder = Folder.ToFolder(folderPath);

            var github = new GitHubHelper();
            github.Upload(folder);

            Console.Read();
        }

        private static void GitLogic()
        {
            var token = "0ed16dbfa7996251b394b8735ccdc9f12aa221ac";

            var client = new GitHubClient(new ProductHeaderValue("amit-test-app"));

            var tokenAuth = new Credentials(token);
            client.Credentials = tokenAuth;

            var owner = "amitaroralive";

            var reponame = "api-repo-2";

            var organizationLogin = "amitgallery";

            //var newRepo = new NewRepository(reponame)
            //{

            //    AutoInit = true // very helpful!

            //};

            var getRepo = client.Repository.Get(organizationLogin, reponame);
            getRepo.Wait();

            //var repository = client.Repository.Create(organizationLogin, newRepo);
            //repository.Wait();

            Console.WriteLine("Browse the repository at: " + getRepo.Result.HtmlUrl);

            client.Repository.Collaborator.Add(getRepo.Result.Id, "amitaroralive2");

            //2 - create a blob containing the contents of our README

            var newBlob = new NewBlob()
            {

                Content = "#MY AWESOME REPO\rthis is some code\rI made it on: " + DateTime.Now.ToString(),
                Encoding = EncodingType.Utf8
            };

            var createdBlob = client.Git.Blob.Create(getRepo.Result.Id, newBlob);
            createdBlob.Wait();


            // 3 - create a tree which represents just the README file
            var newTree = new NewTree();
            newTree.Tree.Add(new NewTreeItem()
            {

                Path = "test/README.md",
                Mode = Octokit.FileMode.File,
                Sha = createdBlob.Result.Sha,
                Type = TreeType.Blob
            });



            var createdTree = client.Git.Tree.Create(getRepo.Result.Id, newTree);
            createdTree.Wait();


            // 4 - this commit should build on the current master branch
            var master = client.Git.Reference.Get(getRepo.Result.Id, "heads/master");
            master.Wait();
            var newCommit = new NewCommit(
                "Hello World!",
                createdTree.Result.Sha,
              new[] { master.Result.Object.Sha })
            { Author = new Committer("ram", "ram@amit.com", DateTime.UtcNow) };



            var createdCommit = client.Git.Commit.Create(getRepo.Result.Id, newCommit);
            createdCommit.Wait();

            // 5 - create a reference for the master branch

            var updateReference = new ReferenceUpdate(createdCommit.Result.Sha);

            var updatedReference = client.Git.Reference.Update(getRepo.Result.Id, "heads/master", updateReference);
            updatedReference.Wait();

            Console.Read();
        }
    }
}
