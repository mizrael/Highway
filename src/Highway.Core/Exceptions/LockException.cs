using System;

namespace Highway.Core.Exceptions
{
    public class LockException : Exception
    {
        public LockException(string msg) : base(msg)
        {
        }
    }
}