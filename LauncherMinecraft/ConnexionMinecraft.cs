using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LauncherMinecraft
{
    class ConnexionMinecraft
    {
        public enum LoginResult { OtherError, ServiceUnavailable, SSLError, Success, WrongPassword, AccountMigrated, NotPremium, LoginRequired, InvalidToken, NullError };

        /// <summary>
        /// Allows to login to a premium Minecraft account using the Yggdrasil authentication scheme.
        /// </summary>
        /// <param name="user">Login</param>
        /// <param name="pass">Password</param>
        /// <param name="accesstoken">Will contain the access token returned by Minecraft.net, if the login is successful</param>
        /// <param name="clienttoken">Will contain the client token generated before sending to Minecraft.net</param>
        /// <param name="uuid">Will contain the player's PlayerID, needed for multiplayer</param>
        /// <returns>Returns the status of the login (Success, Failure, etc.)</returns>

        public static LoginResult GetLogin(string user, string pass, out SessionToken session)
        {
            session = new SessionToken() { ClientID = Guid.NewGuid().ToString().Replace("-", "") };

            try
            {
                string result = "";

                string json_request = "{\"agent\": { \"name\": \"Minecraft\", \"version\": 1 }, \"username\": \"" + jsonEncode(user) + "\", \"password\": \"" + jsonEncode(pass) + "\", \"clientToken\": \"" + jsonEncode(session.ClientID) + "\" }";
                int code = doHTTPSPost("authserver.mojang.com", "/authenticate", json_request, ref result);
                if (code == 200)
                {
                    if (result.Contains("availableProfiles\":[]}"))
                    {
                        return LoginResult.NotPremium;
                    }
                    else
                    {
                        string[] temp = result.Split(new string[] { "accessToken\":\"" }, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length >= 2) { session.ID = temp[1].Split('"')[0]; }
                        temp = result.Split(new string[] { "name\":\"" }, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length >= 2) { session.PlayerName = temp[1].Split('"')[0]; }
                        temp = result.Split(new string[] { "availableProfiles\":[{\"id\":\"" }, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length >= 2) { session.PlayerID = temp[1].Split('"')[0]; }
                        return LoginResult.Success;
                    }
                }
                else if (code == 403)
                {
                    if (result.Contains("UserMigratedException"))
                    {
                        return LoginResult.AccountMigrated;
                    }
                    else return LoginResult.WrongPassword;
                }
                else if (code == 503)
                {
                    return LoginResult.ServiceUnavailable;
                }
                else
                {
                    return LoginResult.OtherError;
                }
            }
            catch (System.Security.Authentication.AuthenticationException)
            {
                return LoginResult.SSLError;
            }
            catch (System.IO.IOException e)
            {
                if (e.Message.Contains("authentication"))
                {
                    return LoginResult.SSLError;
                }
                else return LoginResult.OtherError;
            }
            catch
            {
                return LoginResult.OtherError;
            }
        }

        /// <summary>
        /// Make a HTTPS POST request to the specified endpoint of the Mojang API
        /// </summary>
        /// <param name="host">Host to connect to</param>
        /// <param name="endpoint">Endpoint for making the request</param>
        /// <param name="request">Request payload</param>
        /// <param name="result">Request result</param>
        /// <returns>HTTP Status code</returns>

        private static int doHTTPSPost(string host, string endpoint, string request, ref string result)
        {
            List<String> http_request = new List<string>();
            http_request.Add("POST " + endpoint + " HTTP/1.1");
            http_request.Add("Host: " + host);
            //http_request.Add("User-Agent: MCC/" + Program.Version);
            http_request.Add("Content-Type: application/json");
            http_request.Add("Content-Length: " + Encoding.ASCII.GetBytes(request).Length);
            http_request.Add("Connection: close");
            http_request.Add("");
            http_request.Add(request);
            return doHTTPSRequest(http_request, host, ref result);
        }


        /// <summary>
        /// Manual HTTPS request since we must directly use a TcpClient because of the proxy.
        /// This method connects to the server, enables SSL, do the request and read the response.
        /// </summary>
        /// <param name="headers">Request headers and optional body (POST)</param>
        /// <param name="host">Host to connect to</param>
        /// <param name="result">Request result</param>
        /// <returns>HTTP Status code</returns>

        private static int doHTTPSRequest(List<string> headers, string host, ref string result)
        {
            string postResult = null;
            int statusCode = 520;
            AutoTimeout.Perform(() =>
            {
                TcpClient client = new TcpClient(host, 443);
                SslStream stream = new SslStream(client.GetStream());
                stream.AuthenticateAsClient(host);
                stream.Write(Encoding.ASCII.GetBytes(String.Join("\r\n", headers.ToArray())));
                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                string raw_result = sr.ReadToEnd();
                if (raw_result.StartsWith("HTTP/1.1"))
                {
                    postResult = raw_result.Substring(raw_result.IndexOf("\r\n\r\n") + 4);
                    statusCode = Int32.Parse(raw_result.Split(' ')[1]);
                }
                else statusCode = 520; //Web server is returning an unknown error
            }, TimeSpan.FromSeconds(30));
            result = postResult;
            return statusCode;
        }

        /// <summary>
        /// Encode a string to a json string.
        /// Will convert special chars to \u0000 unicode escape sequences.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <returns>Encoded text</returns>

        private static string jsonEncode(string text)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in text)
            {
                if ((c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z'))
                {
                    result.Append(c);
                }
                else
                {
                    result.AppendFormat(@"\u{0:x4}", (int)c);
                }
            }

            return result.ToString();
        }

        public static string GetJavaInstallationPath()
        {
            string environmentPath = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(environmentPath))
            {
                return environmentPath;
            }

            const string JAVA_KEY = "SOFTWARE\\JavaSoft\\Java Runtime Environment\\";

            var localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var rk = localKey.OpenSubKey(JAVA_KEY))
            {
                if (rk != null)
                {
                    string currentVersion = rk.GetValue("CurrentVersion").ToString();
                    using (var key = rk.OpenSubKey(currentVersion))
                    {
                        return key.GetValue("JavaHome").ToString();
                    }
                }
            }

            localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
            using (var rk = localKey.OpenSubKey(JAVA_KEY))
            {
                if (rk != null)
                {
                    string currentVersion = rk.GetValue("CurrentVersion").ToString();
                    using (var key = rk.OpenSubKey(currentVersion))
                    {
                        return key.GetValue("JavaHome").ToString();
                    }
                }
            }

            return null;
        }
    }
}
