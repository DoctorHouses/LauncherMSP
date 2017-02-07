using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Net.Security;
using System.Net.Sockets;

namespace LauncherMinecraftV3
{
    internal class ConnexionMinecraft
    {
        public enum LoginResult
        {
            OtherError,
            ServiceUnavailable,
            SslError,
            Success,
            WrongPassword,
            AccountMigrated,
            NotPremium,
            LoginRequired,
            InvalidToken,
            NullError
        }

        /// <summary>
        /// Allows to login to a premium Minecraft account using the Yggdrasil authentication scheme.
        /// </summary>
        /// <param name="user">Login</param>
        /// <param name="pass">Password</param>
        /// <param name="accesstoken">Will contain the access token returned by Minecraft.net, if the login is successful</param>
        /// <param name="clienttoken">Will contain the client token generated before sending to Minecraft.net</param>
        /// <param name="uuid">Will contain the player's PlayerID, needed for multiplayer</param>
        /// <param name="session"></param>
        /// <returns>Returns the status of the login (Success, Failure, etc.)</returns>
        public static LoginResult GetLogin(string user, string pass, out SessionToken session)
        {
            session = new SessionToken {ClientId = Guid.NewGuid().ToString().Replace("-", "")};

            try
            {
                string result = "";

                string jsonRequest = "{\"agent\": { \"name\": \"Minecraft\", \"version\": 1 }, \"username\": \"" +
                                      JsonEncode(user) + "\", \"password\": \"" + JsonEncode(pass) +
                                      "\", \"clientToken\": \"" + JsonEncode(session.ClientId) + "\" }";
                int code = DoHttpsPost("authserver.mojang.com", "/authenticate", jsonRequest, ref result);
                switch (code)
                {
                    case 200:
                        if (result.Contains("availableProfiles\":[]}"))
                        {
                            return LoginResult.NotPremium;
                        }
                        string[] temp = result.Split(new[] {"accessToken\":\""},
                            StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length >= 2)
                        {
                            session.Id = temp[1].Split('"')[0];
                        }
                        temp = result.Split(new[] {"name\":\""}, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length >= 2)
                        {
                            session.PlayerName = temp[1].Split('"')[0];
                        }
                        temp = result.Split(new[] {"availableProfiles\":[{\"id\":\""},
                            StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length >= 2)
                        {
                            session.PlayerId = temp[1].Split('"')[0];
                        }
                        return LoginResult.Success;
                    case 403:
                        return result.Contains("UserMigratedException") ? LoginResult.AccountMigrated : LoginResult.WrongPassword;
                    case 503:
                        return LoginResult.ServiceUnavailable;
                    default:
                        return LoginResult.OtherError;
                }
            }
            catch (System.Security.Authentication.AuthenticationException)
            {
                return LoginResult.SslError;
            }
            catch (System.IO.IOException e)
            {
                return e.Message.Contains("authentication") ? LoginResult.SslError : LoginResult.OtherError;
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

        private static int DoHttpsPost(string host, string endpoint, string request, ref string result)
        {
            List<string> httpRequest = new List<string>
            {
                "POST " + endpoint + " HTTP/1.1",
                "Host: " + host,
                "Content-Type: application/json",
                "Content-Length: " + Encoding.ASCII.GetBytes(request).Length,
                "Connection: close",
                "",
                request
            };
            //http_request.Add("User-Agent: MCC/" + Program.Version);
            return DoHttpsRequest(httpRequest, host, ref result);
        }


        /// <summary>
        /// Manual HTTPS request since we must directly use a TcpClient because of the proxy.
        /// This method connects to the server, enables SSL, do the request and read the response.
        /// </summary>
        /// <param name="headers">Request headers and optional body (POST)</param>
        /// <param name="host">Host to connect to</param>
        /// <param name="result">Request result</param>
        /// <returns>HTTP Status code</returns>

        private static int DoHttpsRequest(List<string> headers, string host, ref string result)
        {
            string postResult = null;
            int statusCode = 520;
            AutoTimeout.Perform(() =>
            {
                TcpClient client = new TcpClient(host, 443);
                SslStream stream = new SslStream(client.GetStream());
                stream.AuthenticateAsClient(host);
                stream.Write(Encoding.ASCII.GetBytes(string.Join("\r\n", headers.ToArray())));
                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                string rawResult = sr.ReadToEnd();
                if (rawResult.StartsWith("HTTP/1.1"))
                {
                    postResult = rawResult.Substring(rawResult.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4);
                    statusCode = int.Parse(rawResult.Split(' ')[1]);
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

        private static string JsonEncode(string text)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in text)
            {
                if (c >= '0' && c <= '9' ||
                    c >= 'a' && c <= 'z' ||
                    c >= 'A' && c <= 'Z')
                {
                    result.Append(c);
                }
                else
                {
                    result.AppendFormat(@"\u{0:x4}", (int) c);
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

            const string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment\\";

            RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using (RegistryKey rk = localKey.OpenSubKey(javaKey))
            {
                if (rk != null)
                {
                    string currentVersion = rk.GetValue("CurrentVersion").ToString();
                    using (RegistryKey key = rk.OpenSubKey(currentVersion))
                    {
                        if (key != null) return key.GetValue("JavaHome").ToString();
                    }
                }
            }

            localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (RegistryKey rk = localKey.OpenSubKey(javaKey))
            {
                if (rk == null) return null;
                string currentVersion = rk.GetValue("CurrentVersion").ToString();
                using (RegistryKey key = rk.OpenSubKey(currentVersion))
                {
                    if (key != null) return key.GetValue("JavaHome").ToString();
                }
            }



            return null;
        }

    }
}
