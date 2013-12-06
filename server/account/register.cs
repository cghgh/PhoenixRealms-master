using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using db;
using MySql.Data.MySqlClient;

namespace server.account
{
    internal class register : IRequestHandler
    {
        public void HandleRequest(HttpListenerContext context)
        {
            NameValueCollection query;
            using (var rdr = new StreamReader(context.Request.InputStream))
                query = HttpUtility.ParseQueryString(rdr.ReadToEnd());

            using (var db = new Database())
            {
                byte[] status;
                if (!IsValidUsername(query["newGUID"]))
                    status = Encoding.UTF8.GetBytes("<Error>Invalid Username</Error>");
                if (db.HasUuid(query["guid"]) &&
                    db.Verify(query["guid"], "") != null)
                {
                    if (db.HasUuid(query["newGUID"]))
                        status = Encoding.UTF8.GetBytes("<Error>Username is already taken!</Error>");
                    else
                    {
                        MySqlCommand cmd = db.CreateQuery();
                        cmd.CommandText =
                            "UPDATE accounts SET uuid=@newUuid, name=@newUuid, password=SHA1(@password), guest=FALSE WHERE uuid=@uuid, name=@name;";
                        cmd.Parameters.AddWithValue("@uuid", query["guid"]);
                        cmd.Parameters.AddWithValue("@newUuid", query["newGUID"]);
                        cmd.Parameters.AddWithValue("@password", query["newPassword"]);

                        if (cmd.ExecuteNonQuery() > 0)
                            status = Encoding.UTF8.GetBytes("<Success />");
                        else
                            status = Encoding.UTF8.GetBytes("<Error>Internal Error</Error>");
                    }
                }
                else
                {
                    if (db.Register(query["newGUID"], query["newPassword"], false) != null)
                        status = Encoding.UTF8.GetBytes("<Success />");
                    else
                        status = Encoding.UTF8.GetBytes("<Error>Internal Error</Error>");
                }
                context.Response.OutputStream.Write(status, 0, status.Length);
                db.Dispose();
            }
        }

        public bool IsValidUsername(string strIn)
        {
            bool invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            MatchEvaluator DomainMapper = match =>
            {
                // IdnMapping class with default property values.
                var idn = new IdnMapping();

                string domainName = match.Groups[2].Value;
                try
                {
                    domainName = idn.GetAscii(domainName);
                }
                catch (ArgumentException)
                {
                    invalid = true;
                }
                return match.Groups[1].Value + domainName;
            };

            // Use IdnMapping class to convert Unicode domain names. 
            // strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper);
            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            return Regex.IsMatch(strIn,
                @"^[a-z][a-zA-Z]*$",
                RegexOptions.IgnoreCase);
        }
    }
}