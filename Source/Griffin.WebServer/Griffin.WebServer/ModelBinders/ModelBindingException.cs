using System;

namespace Griffin.WebServer.ModelBinders
{
    public class ModelBindingException : Exception
    {
        public string FullModelName { get; private set; }
        public object Value { get; private set; }

        public ModelBindingException(string message, string fullModelName, object value)
            :base(message)
        {
            FullModelName = fullModelName;
            Value = value;
        }

        public ModelBindingException(string message)
            : base(message)
        {
        }

        public ModelBindingException(string message, string fullModelName, object value, Exception inner)
            : base(message, inner)
        {
            FullModelName = fullModelName;
            Value = value;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        public override string Message
        {
            get { return string.Format("Binding failure '{0}' = '{1}', Error: {2}", FullModelName, Value, base.Message); }
        }
    }
}