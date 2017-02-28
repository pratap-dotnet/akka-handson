using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console_actors
{
    #region Neutral/system Messages
    public class ContinueProcessing { }
    #endregion

    #region Success Messages
    public class InputSuccess
    {
        public string Reason { get; private set; }

        public InputSuccess(string reason)
        {
            Reason = reason;
        }
    }
    #endregion

    #region Error Messages
    public class InputError
    {
        public string Reason { get; private set; }
        public InputError(string reason)
        {
            Reason = reason;
        }
    }

    public class NullInputError : InputError
    {
        public NullInputError(string reason) : base(reason)
        {

        }
    }

    public class ValidationError : InputError
    {
        public ValidationError(string reason): base(reason)
        {

        }
    }
    #endregion

    class Messages
    {
    }
}
