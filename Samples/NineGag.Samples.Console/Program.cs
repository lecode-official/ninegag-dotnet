
#region Using Directives

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace NineGag.Samples.Console
{
    /// <summary>
    /// Represents the entry point to the sample application, which showcases the usage of 9GAG.NET.
    /// </summary>
    public class Program
    {
        #region Public Static Methods

        /// <summary>
        /// The entry point to the 9GAG.NET sample application.
        /// </summary>
        /// <param name="args">The command line arguments, which should be empty, because they are not used.</param>
        public static void Main(string[] args) => Program.MainAsync().Wait();

        /// <summary>
        /// Represents the asynchronous entry point to the application, which makes it possible to call asynchronous methods.
        /// </summary>
        public static async Task MainAsync()
        {
            // Creates a new 9GAG client and tries to access the 9GAG posts
            try
            {
                using (NineGagClient nineGagClient = new NineGagClient())
                {
                    // Signs the user in
                    System.Console.Write("Email address: ");
                    string emailAddress = System.Console.ReadLine();
                    System.Console.Write("Password: ");
                    string password = System.Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(emailAddress) && !string.IsNullOrWhiteSpace(password))
                        await nineGagClient.SignInAsync(emailAddress, password);

                    // Gets the first two pages of 9GAG
                    List<Page> pages = new List<Page>();
                    pages.Add(await nineGagClient.GetPostsAsync());
                    pages.Add(await nineGagClient.GetPostsAsync(pages.Last()));

                    // Prints all the retrieved pages
                    foreach (Page page in pages)
                    {
                        System.Console.WriteLine();
                        System.Console.WriteLine($"Page {pages.IndexOf(page) + 1}");
                        System.Console.WriteLine();

                        foreach (Post post in page.Posts)
                            System.Console.WriteLine(post.Title);
                    }
                    
                    // Waits for a key stroke before the application is quit
                    System.Console.WriteLine();
                    System.Console.WriteLine("Press any key to exit...");
                    System.Console.ReadKey();

                    // Signs the user out
                    await nineGagClient.SignOutAsync();
                }
            }
            catch (NineGagException exception)
            {
                System.Console.WriteLine($"An error occurred, while retrieving the 9GAG posts: '{exception.Message}'.");
            }
        }

        #endregion
    }
}