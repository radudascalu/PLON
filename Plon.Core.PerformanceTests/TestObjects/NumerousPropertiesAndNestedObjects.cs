using System;

namespace Plon.Core.PerformanceTests.TestObjects
{
    class NumerousPropertiesAndNestedObjects
    {
        public string Url { get; set; }
        public string Sha { get; set; }
        public string HtmlUrl { get; set; }
        public string CommentsUrl { get; set; }
        public bool Private { get; set; }
        public string Description { get; set; }
        public bool Fork { get; set; }
        public decimal Score { get; set; }

        public CommitInfo Commit { get; set; }
        public User Author { get; set; }
        public RepositoryInfo Repository { get; set; }

        public static NumerousPropertiesAndNestedObjects New()
        {
            return new NumerousPropertiesAndNestedObjects
            {
                Url = "https://api.github.com/repos/octocat/Spoon-Knife/commits/bb4cc8d3b2e14b3af5df699876dd4ff3acd00b7f",
                Sha = "bb4cc8d3b2e14b3af5df699876dd4ff3acd00b7f",
                HtmlUrl = "https://github.com/octocat/Spoon-Knife/commit/bb4cc8d3b2e14b3af5df699876dd4ff3acd00b7f",
                CommentsUrl = "https://api.github.com/repos/octocat/Spoon-Knife/commits/bb4cc8d3b2e14b3af5df699876dd4ff3acd00b7f/comments",
                Private = false,
                Description = "This repo is for demonstration purposes only.",
                Fork = false,
                Score = 4.9971514m,
                Commit = new CommitInfo
                {
                    Url = "https://api.github.com/repos/octocat/Spoon-Knife/git/commits/bb4cc8d3b2e14b3af5df699876dd4ff3acd00b7f",
                    Author = new UserInfo
                    {
                        Date = DateTime.Now,
                        Name = "The Octocat",
                        Email = "octocat@nowhere.com"
                    },
                    Committer = new UserInfo
                    {
                        Date = DateTime.Now,
                        Name = "The Octocat",
                        Email = "octocat@nowhere.com"
                    },
                    CommentCount = 8
                },
                Author = new User
                {
                    Login = "octocat",
                    Id = 583231,
                    NodeId = "MDQ6VXNlcjU4MzIzMQ==",
                    AvatarUrl = "https://avatars.githubusercontent.com/u/583231?v=3",
                    GravatarId = "",
                    Url = "https://api.github.com/users/octocat",
                    SiteAdmin = false
                },
                Repository = new RepositoryInfo
                {

                    Id = 1300192,
                    NodeId = "MDEwOlJlcG9zaXRvcnkxMzAwMTky",
                    Name = "Spoon-Knife",
                    FullName = "octocat/Spoon-Knife",
                    Owner = new User
                    {
                        Login = "octocat",
                        Id = 583231,
                        NodeId = "MDQ6VXNlcjU4MzIzMQ==",
                        AvatarUrl = "https://avatars.githubusercontent.com/u/583231?v=3",
                        GravatarId = "",
                        Url = "https://api.github.com/users/octocat",
                        SiteAdmin = false
                    }
                }
            };
        }
    }


    class User
    {
        public string Login { get; set; }
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string AvatarUrl { get; set; }
        public string GravatarId { get; set; }
        public string Url { get; set; }
        public bool SiteAdmin { get; set; }
    }

    class CommitInfo
    {
        public string Url { get; set; }
        public UserInfo Author { get; set; }
        public UserInfo Committer { get; set; }
        public int CommentCount { get; set; }
    }

    class UserInfo
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    class RepositoryInfo
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public User Owner { get; set; }
    }
}
