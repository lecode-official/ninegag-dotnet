
#region Using Directives

using System;
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
        public static async Task Main(string[] args)
        {
            // Creates a new 9GAG client, retrieves all sections, and prints the posts of the first section
            using (NineGagClient client = new NineGagClient())
            {
                foreach (Post post in await client.GetHotPostsAsync())
                    Console.WriteLine(post.Title);
            }
        }

        #endregion
    }
}
