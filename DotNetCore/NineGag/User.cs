
#region Using Directives

using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using System;
using System.Linq;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a user on 9GAG.
    /// </summary>
    public class User
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="User"/> instance. The constructor is private, because <see cref="User"/> implements a factory pattern.
        /// </summary>
        private User() { }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string EmailAddress { get; private set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets or sets the URI of the avatar picture of the user.
        /// </summary>
        public Uri AvatarUri { get; private set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Parses the account and profile settings page of the user and creates a user out of it.
        /// </summary>
        /// <param name="accountSettingsPage">The account settings page, which contains the user name and the email address of the user.</param>
        /// <param name="profileSettingsPage">The profile settings page, which contains the full name and the avatar of the user.</param>
        /// <returns>Returns the created user with the parsed information.</returns>
        public static User FromHtml(IHtmlDocument accountSettingsPage, IHtmlDocument profileSettingsPage)
        {
            // Creates a new user
            User user = new User();

            // Tries to parse the account settings page for the user name and the email address, if it could not be parsed, then an exception is thrown
            try
            {
                IElement accountSettingsForm = accountSettingsPage.QuerySelector("#setting");
                user.UserName = accountSettingsForm.QuerySelectorAll("input").FirstOrDefault(input => input.GetAttribute("name") == "login_name").GetAttribute("value");
                user.EmailAddress = accountSettingsForm.QuerySelectorAll("input").FirstOrDefault(input => input.GetAttribute("name") == "email").GetAttribute("value");
            }
            catch (Exception exception)
            {
                throw new NineGagException("The user name and the email address could not be parsed. This could be an indicator, that the 9GAG website is down or its content has changed. If this problem keeps coming, then please report this problem to 9GAG or the maintainer of the library.", exception);
            }

            // Tries to parse the profile settings page for the full name and the avatar image of the user, if it could not be parsed, then an exception is thrown
            try
            {
                user.FullName = profileSettingsPage.QuerySelectorAll("input").FirstOrDefault(input => input.GetAttribute("name") == "fullName").GetAttribute("value");
                user.AvatarUri = new Uri(profileSettingsPage.QuerySelector("#jsid-profile-avatar").GetAttribute("src"), UriKind.Absolute);
            }
            catch (Exception exception)
            {
                throw new NineGagException("The full name and the avatar image could not be parsed. This could be an indicator, that the 9GAG website is down or its content has changed. If this problem keeps coming, then please report this problem to 9GAG or the maintainer of the library.", exception);
            }

            // Returns the created user
            return user;
        }

        #endregion
    }
}