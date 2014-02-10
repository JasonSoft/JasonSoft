using System;
#if !(WindowsCE)
using System.Runtime.Serialization;
#endif

namespace JasonSoft.Components.SmartThreadPool
{
    #region Exceptions

    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been canceled
    /// </summary>
    public sealed partial class WorkItemCancelException : ApplicationException
    {
        public WorkItemCancelException()
        {
        }

        public WorkItemCancelException(string message)
            : base(message)
        {
        }

        public WorkItemCancelException(string message, Exception e)
            : base(message, e)
        {
        }
    }

    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been timed out
    /// </summary>
    public sealed partial class WorkItemTimeoutException : ApplicationException
    {
        public WorkItemTimeoutException()
        {
        }

        public WorkItemTimeoutException(string message)
            : base(message)
        {
        }

        public WorkItemTimeoutException(string message, Exception e)
            : base(message, e)
        {
        }
    }

    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been timed out
    /// </summary>
    public sealed partial class WorkItemResultException : ApplicationException
    {
        public WorkItemResultException()
        {
        }

        public WorkItemResultException(string message)
            : base(message)
        {
        }

        public WorkItemResultException(string message, Exception e)
            : base(message, e)
        {
        }
    }


#if !(WindowsCE)
    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been canceled
    /// </summary>
    [Serializable]
    public sealed partial class WorkItemCancelException
    {
        public WorkItemCancelException(SerializationInfo si, StreamingContext sc)
            : base(si, sc)
        {
        }
    }

    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been timed out
    /// </summary>
    [Serializable]
    public sealed partial class WorkItemTimeoutException
    {
        public WorkItemTimeoutException(SerializationInfo si, StreamingContext sc)
            : base(si, sc)
        {
        }
    }

    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been timed out
    /// </summary>
    [Serializable]
    public sealed partial class WorkItemResultException
    {
        public WorkItemResultException(SerializationInfo si, StreamingContext sc)
            : base(si, sc)
        {
        }
    }

#endif

    #endregion
}
