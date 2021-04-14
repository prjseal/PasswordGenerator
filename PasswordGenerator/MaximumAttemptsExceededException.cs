using System;
using System.Runtime.Serialization;

namespace PasswordGenerator
{
    /// <summary>
    /// Represents errors that occurs during random password generation when the configured maximum attempts are exceeded.
    /// </summary>
    [Serializable]
    public sealed class MaximumAttemptsExceededException : Exception
    {
        public MaximumAttemptsExceededException()
        {
        }

        private MaximumAttemptsExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}