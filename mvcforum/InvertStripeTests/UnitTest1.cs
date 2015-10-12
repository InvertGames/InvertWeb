using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MVCForum.Services;
using Octokit;
using Stripe;

namespace InvertStripeTests
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod1()
        {
            //Task.Run(async () =>
            //{
            //    var client = new GitHubClient(new ProductHeaderValue("InvertGameStudios.com"))
            //    {
            //        Credentials = new Credentials("micahosborne@gmail.com", "micah123")
            //    };
            //    var organization = await client.Organization.Team.GetAll();
            //    organization.
            //    Console.WriteLine(organization.Id);
            //    client.Organization.Team.
            //    var users = await client.Search.SearchUsers(new SearchUsersRequest("micahosborne@gmail.com"));

            //    foreach (var item in users.Items)
            //    {
            //        Console.WriteLine(item.Id);
            //    }
            //    Console.WriteLine("Done");
            //    // Actual test code here.
            //}).GetAwaiter().GetResult();
     
            //client.Organization.Team.AddMembership()
        }
    }
}
