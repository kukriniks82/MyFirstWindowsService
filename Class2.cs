using System;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace MiFirstService
{
    public class KeyRequst
    {
        ConfigJson conf = new ConfigJson();
        Logging log = new Logging();

        public Tuple<int, string> GetSekretKey()
        {

            Settings settings = (Settings)conf.GetSettings();
            try
            {
                string Stringdata = $"client_id=custom_service&client_host={Dns.GetHostName()}";
                byte[] data = Encoding.UTF8.GetBytes(Stringdata);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create($"http://{settings.ServerAdress}:{settings.Port}/api/v1/oauth2/register");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                request.Credentials = CredentialCache.DefaultCredentials;
                request.ContentLength = data.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();


                
                // Display the status.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //log.log("GetSKey1 " + response.StatusDescription);
                log.log("GetSKey " + response.StatusCode.ToString());

                // The using block ensures the stream is automatically closed.
                using (requestStream = response.GetResponseStream())
                {
               
                    StreamReader reader = new StreamReader(requestStream);
                        string responseFromServer = reader.ReadToEnd();
                    

                    JsonSerializer RespSerializer = new JsonSerializer();
                    
                    dynamic json = JsonConvert.DeserializeObject(responseFromServer);

                    string Skey = json.secret_key;
                    response = (HttpWebResponse)request.GetResponse();
                    
                    response.Close();
                    log.log("SKey " + Skey);

                    //string resp = ((int)response.StatusCode).ToString();

                    return Tuple.Create((int)response.StatusCode, Skey);

                }


              

            }
            catch (WebException we)
            {
                String Skey = null;
                HttpWebResponse response = (HttpWebResponse)we.Response;
                if (we.Response != null)
                {
                    log.log("GetSKey stausCode " + ((int)response.StatusCode).ToString());
                    log.log("GetSKey" + we.Message.ToString());
                    log.log("GetSKey" + we.Status.ToString());
                    return Tuple.Create((int)response.StatusCode, Skey); //Status code, 409 for exmple 
                }

                
                log.log("GetSKey: " + we.Message.ToString() + " " + settings.ServerAdress + ":" + settings.Port);
                return null;




            }

        }
        public Tuple<int, string> GetToken()
        {
            ConfigJson conf = new ConfigJson();
            Logging log = new Logging();
            Settings settings = (Settings)conf.GetSettings();
            try
            {
                string Stringdata = $"grant_type=client_credentials&client_host={Dns.GetHostName()}";
                byte[] data = Encoding.UTF8.GetBytes(Stringdata);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create($"http://{settings.ServerAdress}:{settings.Port}/api/v1/oauth2/token");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                request.Credentials = CredentialCache.DefaultCredentials;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("custom_service" + ":" + settings.SKey));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.ContentLength = data.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();


                //WebResponse response = request.GetResponse();
                // Display the status.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //log.log("GetToken1 " + response.StatusDescription);
                //log.log("GetToken1 " + response.StatusCode.ToString());

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (requestStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(requestStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.


                    JsonSerializer RespSerializer = new JsonSerializer();
                    //SKeyResponse resp = (Settings)RespSerializer.Deserialize(responseFromServer, typeof(SKeyResponse));
                    dynamic json = JsonConvert.DeserializeObject(responseFromServer);

                    string Skey = json.access_token;
                    response = (HttpWebResponse)request.GetResponse();
                    // This will have statii from 200 to 30x
                    response.Close();
                    //log.log("access_token " + Skey);

                    //string resp = ((int)response.StatusCode).ToString();

                    return Tuple.Create((int)response.StatusCode, Skey);

                }


                // Close the response.

            }
            catch (WebException we)
            {
                String Skey = null;
                HttpWebResponse response = (HttpWebResponse)we.Response;
                if (we.Response != null)
                {
                    log.log("Token stausCode " + ((int)response.StatusCode).ToString());
                    log.log("GetToken " + we.Message.ToString());
                    //log.log("GetToken4" + we.Status.ToString());
                    return Tuple.Create((int)response.StatusCode, Skey); //Status code, 409 for exmple 
                }

                return null;
            }

        }

        public int CheckToken()
        {
            ConfigJson conf = new ConfigJson();
            Logging log = new Logging();
            Settings settings = (Settings)conf.GetSettings();
            try
            {
                string Stringdata = $"grant_type=client_credentials&client_host={Dns.GetHostName()}";
                byte[] data = Encoding.UTF8.GetBytes(Stringdata);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create($"http://{settings.ServerAdress}:{settings.Port}/api/v1/oauth2/check_token");
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Headers.Add("Authorization", "Bearer " + settings.Token);


                //WebResponse response = request.GetResponse();
                // Display the status.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //log.log("ChecToken8 " + response.StatusDescription);
                //log.log("ChecToken9 " + response.StatusCode.ToString());

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.

                response = (HttpWebResponse)request.GetResponse();
                // This will have statii from 200 to 30x
                response.Close();

                //string resp = ((int)response.StatusCode).ToString();

                return (int)response.StatusCode;

            }

            // Close the response.


            catch (WebException we)
            {

                HttpWebResponse response = (HttpWebResponse)we.Response;
                if (we.Response != null)
                {
                    //log.log("ChekToken1 stausCode " + ((int)response.StatusCode).ToString());
                    log.log("ChekToken2" + we.Message.ToString());
                    //log.log("ChekToken3" + we.Status.ToString());
                    return (int)response.StatusCode; //Status code, 409 for exmple 
                }

                // log.log("ChekToken5" + we.Status.ToString());
                log.log("ChekToken " + we.Message.ToString() + " " + settings.ServerAdress + ":" + settings.Port);
                return 0;
            }

        }
    }

}
