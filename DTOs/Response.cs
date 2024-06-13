using System.Net;
using System.Text;

namespace codecrafters_http_server.DTOs
{
    internal class Response
    {
        internal string? Protocol { get; set; }
        internal HttpStatusCode? HttpStatusCode { get; set; }
        internal string? ContentType { get; set; }
        internal int? ContentLength { get; set; }
        internal object? Content { get; set; }
        internal string? ContentEncoding { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new();
            var httpStatusCodeString = HttpStatusCode.ToString();

            if(httpStatusCodeString == System.Net.HttpStatusCode.NotFound.ToString())
            {
                httpStatusCodeString = "Not Found";
            }
            
            sb.Append($"{Protocol} {(int)HttpStatusCode!} {httpStatusCodeString}{Constants.Constants.Crlf}");

            if(!string.IsNullOrEmpty(ContentType))
            {
                sb.Append($"Content-Type: {ContentType}{Constants.Constants.Crlf}");
            }

            if (!string.IsNullOrEmpty(ContentEncoding))
            {
                sb.Append($"Content-Encoding: {ContentEncoding}{Constants.Constants.Crlf}");
            }

            if (ContentLength != null)
            {
                sb.Append($"Content-Length: {ContentLength}{Constants.Constants.Crlf}{Constants.Constants.Crlf}{Content}");
            }

            if(string.IsNullOrEmpty(ContentType) && ContentLength == null && string.IsNullOrEmpty(ContentEncoding))
            {
                sb.Append(Constants.Constants.Crlf);
            }


            return sb.ToString();
        }
    }
}
