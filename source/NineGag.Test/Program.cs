
#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace NineGag.Test
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
            // Creates a new 9GAG client, retrieves all sections, and prints the posts of the first section
            using (NineGagClient client = new NineGagClient())
            {
                IEnumerable<Section> sections = await client.GetSectionsAsync();
                IEnumerable<string> posts = await client.GetPostsAsync(sections.First());
                foreach (string post in posts)
                    Console.WriteLine(post);
            }
        }

        #endregion
    }
}
