using System;


using Box.V2.Config;
using Box.V2.JWTAuth;
using System.IO;
using System.Threading.Tasks;
using Box.V2.Auth.Token;
using Microsoft.Extensions.Configuration;
using Box.V2;
using Box.V2.Models;
using System.Collections.Generic;

namespace DotNetAsUserJWT
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }
        static void Main(string[] args)
        {
            Task t = MainAsync();
            t.Wait();

            Console.WriteLine();
            Console.Write("Press return to exit...");
            Console.ReadLine();
        }
        static async Task MainAsync()
        {
            try
            {
                /* Read the config file that is provided when an application is
                 * created in the Box Dev Consolse
                 * */

                string jsonConfig = System.IO.File.ReadAllText(configFile());
                var config = BoxConfig.CreateFromJsonString(jsonConfig);
                /* Authenticate. This will provide access to the service account */
                var boxJWT = new BoxJWTAuth(config);
                var adminToken = "";
                adminToken = boxJWT.AdminToken();
                Console.WriteLine("Admin Token:" + adminToken);
                /*
                * Searching for a particular user from the enterprise given the login name
                */
                BoxClient boxClient = boxJWT.AdminClient(adminToken);
                BoxCollection<BoxUser> boxUserCollection = await boxClient.UsersManager.GetEnterpriseUsersAsync(userLogin(), 0, 100, null, "managed", null, false);
                List<BoxUser> userList = boxUserCollection.Entries;
                Console.WriteLine("Entries:" + userList.Count);
                if (userList.Count > 0)
                {
                    foreach (var user in userList)
                    {
                        Console.WriteLine("User Login:" + user.Name + " ID:" + user.Id);
                    }
                }

                /* Replace this variable for the user you want. This is the users
                 * internal Box ID and is all numbers e.g. 3445252385. Suggest that
                 * the list of users in the system is cached in the Token Factory 
                 * and synced perdiodically.
                 */
                var userId = userInformation();
                /* Ask box for a token for the user */
                var userToken = boxJWT.UserToken(userId);

                Console.WriteLine("User Token:" + userToken);
                /* Generate a downscoped token to the ITEM_PREVIEW scope */
                var exchanger = new TokenExchange(adminToken, "item_preview");
                /*Optionally you can downscope to a particular resource. Omitting this will downscope
                * all resources to the scope set above regardless of resource.
                * exchanger.SetResource("https://api.box.com/2.0/files/123456789");
                */
                string downscopedToken = exchanger.Exchange();
                Console.WriteLine("Downscoped ITEM_PREVIEW Token:" + downscopedToken);
                /* Print out some user information for the demo */
                var userClient = boxJWT.UserClient(userToken, userId);

                var userDetails = await userClient.UsersManager.GetCurrentUserInformationAsync();
                Console.WriteLine("\n User Details:");
                Console.WriteLine("\tId: {0}", userDetails.Id);
                Console.WriteLine("\tName: {0}", userDetails.Name);
                Console.WriteLine("\tStatus: {0}", userDetails.Status);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

        }

        static string userInformation()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();


            // Read instrumentation key from IConfiguration.
            string userId = $"{Configuration["userId"]}";
            return userId;

        }

        static string userLogin()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();


            // Read instrumentation key from IConfiguration.
            string userId = $"{Configuration["userLogin"]}";
            return userId;

        }
        static string configFile()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            // Read instrumentation key from IConfiguration.
            string userId = $"{Configuration["configFile"]}";
            return userId;

        }
    }
}
