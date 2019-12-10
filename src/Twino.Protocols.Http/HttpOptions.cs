using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Twino.Protocols.Http
{
    /// <summary>
    /// HTTP Protocol option for Twino HTTP Server
    /// </summary>
    public class HttpOptions
    {
        /// <summary>
        /// Maximum keeping alive duration for each TCP connection
        /// </summary>
        public int HttpConnectionTimeMax { get; set; }
        
        /// <summary>
        /// Maximum request lengths (includes content)
        /// </summary>
        public int MaximumRequestLength { get; set; }
        
        /// <summary>
        /// Supported encodings (Only used when clients accept)
        /// </summary>
        public ContentEncodings[] SupportedEncodings { get; set; }

        /// <summary>
        /// Listening hostnames.
        /// In order to accept all hostnames skip null or set 1-length array with "*" element 
        /// </summary>
        public string[] Hostnames { get; set; }

        /// <summary>
        /// Createsd default HTTP server options
        /// </summary>
        public static HttpOptions CreateDefault()
        {
            return new HttpOptions
                   {
                       HttpConnectionTimeMax = 300,
                       MaximumRequestLength = 1024 * 100,
                       SupportedEncodings = new[]
                                            {
                                                ContentEncodings.Brotli,
                                                ContentEncodings.Gzip
                                            }
                   };
        }

        /// <summary>
        /// Loads options from filename
        /// </summary>
        public static HttpOptions Load(string filename)
        {
            string json = System.IO.File.ReadAllText(filename);
            JObject obj = JObject.Parse(json);

            HttpOptions options = new HttpOptions();

            options.HttpConnectionTimeMax = obj["HttpConnectionTimeMax"].Value<int>();
            options.MaximumRequestLength = obj["MaximumRequestLength"].Value<int>();
            options.Hostnames = obj["Hostnames"].Values<string>().ToArray();

            string[] sx = obj["SupportedEncodings"].Values<string>().ToArray();
            List<ContentEncodings> encodings = new List<ContentEncodings>();
            foreach (string s in sx)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                switch (s.Trim().ToLower())
                {
                    case "none": encodings.Add(ContentEncodings.None); break;
                    case "gzip": encodings.Add(ContentEncodings.Gzip); break;
                    case "br": encodings.Add(ContentEncodings.Brotli); break;
                    case "brotli": encodings.Add(ContentEncodings.Brotli); break;
                    case "deflate": encodings.Add(ContentEncodings.Deflate); break;
                }
            }

            if (encodings.Count == 0)
                encodings.Add(ContentEncodings.None);
            
            options.SupportedEncodings = encodings.ToArray();

            return options;
        }
    }
}