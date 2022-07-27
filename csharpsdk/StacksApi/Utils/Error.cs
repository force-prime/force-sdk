using System;
using System.Net;

namespace StacksForce.Utils
{
    public class Error
    {
        private readonly string _id;

        protected string _info = string.Empty;

        public string Id => _id;
        public string Info => _info;

        public Error(string id, string? info = null)
        {
            _id = id;

            AttachInfo(info);
        }

        public Error(Exception e)
        {
            _id = e.GetType().FullName;

            AttachInfo(e.ToString());
        }

        public void AttachInfo(string? info)
        {
            _info = info != null ? info : string.Empty;
        }

        public override string ToString() {
            return _id + "\n" + _info;
        }
    }

    public class NetworkError: Error
    {
        public NetworkError(string id, Exception exception) : base(id)
        {
            AttachInfo(exception?.Message);
        }
    }

    public class HttpError : Error
    {
        public HttpStatusCode StatusCode { get; }
        public string Content { get; }
        public HttpError(HttpStatusCode statusCode, string content) : base("HttpError")
        {
            StatusCode = statusCode;
            Content = content;

            AttachInfo($"statusCode = {statusCode}, content = {content}");
        }
    }
}
