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
        internal string? Content { get; set; }
        internal string? ContentEncoding { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new();
            
            sb.Append($"{Protocol} {(int)HttpStatusCode!} {HttpStatusCode}{Constants.Constants.Crlf}");

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
