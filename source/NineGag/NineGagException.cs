
#region Using Directives

using System;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents an exception, which is thrown by the <see cref="NineGagClient"/> to signal any errors during the parsing process. Having a single exception type makes error handling much easier.
    /// </summary>
    public class NineGagException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="NineGagException"/> instance.
        /// </summary>
        public NineGagException() { }

        /// <summary>
        /// Initializes a new <see cref="NineGagException"/> instance.
        /// </summary>
        /// <param name="message">The error message, which describes what went wrong during the parsing.</param>
        public NineGagException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new <see cref="NineGagException"/> instance.
        /// </summary>
        /// <param name="message">The error message, which describes what went wrong during the parsing.</param>
        /// <param name="innerException">The original exception, which caused this exception to be thrown.</param>
        public NineGagException(string message, Exception innerException):
            base (message, innerException) { }

        #endregion
    }
}
