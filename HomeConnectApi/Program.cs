using System;
using System.Web;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using RestSharp;
using System.Reflection;

namespace HomeConnectApi
{
    public class AccessToken
    {
        // <summary
        // Generated Access Token 
        // </summary>
        [JsonProperty("access_token")]
        public string accessToken { get; set; }
        // <summary>
        // Refresh Token, neccessary for 'Acces Token Refresh' after expire
        // </summary>
        [JsonProperty("refresh_token")]
        public string refreshToken { get; set; }
        // <summary>
        // Type of the given Token 
        // </summary>
        [JsonProperty("token_type")]
        public string tokenType { get; set; }
        // <summary>
        // Shows the given scope of permissions
        // </summary>
        [JsonProperty("scope")]
        public string scope { get; set; }
        // <summary>
        // Time in seconds the access_token expires
        // </summary>
        [JsonProperty("expires_in")]
        public int expires_in { get; set; }
        // <summary>
        // Same as access_token
        // </summary>
        [JsonProperty("id_token")]
        public string id_token { get; set; }
    }

    public class HomeAppliance
    {
        [JsonProperty("haId")]
        public string haid { get; set; }
        [JsonProperty("vib")]
        public string vib { get; set; }
        [JsonProperty("brand")]
        public string brand { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("enumber")]
        public string enumber { get; set; }
        [JsonProperty("connected")]
        public bool connected { get; set; }
    }

    public class Status
    {
        [JsonProperty("key")]
        public string key { get; set; }
        [JsonProperty("value")]
        public string value { get; set; }
        [JsonProperty("unit")]
        public string unit { get; set; }
    }

    public class Setting
    {
        [JsonProperty("key")]
        public string key { get; set; }
        [JsonProperty("value")]
        public string value { get; set; }
        [JsonProperty("unit")]
        public string unit { get; set; }
    }

    class Program
    {
        // Base URL 
        static string baseUrl = "https://developer.home-connect.com";
        // Client_ID
        static private string client_id = "434AB33977AF19E6B34F7C4FFE7983BF2D7826D4872E8F56AD0782DEAEED2DB2";
        // Redirect URL 
        static private string redirect_url = "https://apiclient.home-connect.com/o2c.html";
        // Content-Type
        static private string content_type = "application/vnd.bsh.sdk.v1+json";
        
        static void Main()
        {             
            Task task = new Task(Start);
            task.Start();
            task.Wait();
            Console.ReadLine();           
        }
        public static async void Start()
        {
            var task = GetAccessToken();
            Console.WriteLine("Please wait...");
            var AccessToken = await task;
            Console.WriteLine("AccessToken = " + AccessToken.accessToken);

            var task1 = EnumerateHomeAppliances(AccessToken.accessToken);
            var homeAppliances = await task1;

            var task2 = GetCurrentStatusOfHomeAppliance(AccessToken.accessToken, homeAppliances[0].haid);
            var StatusList = await task2;

            var task3 = GetAvailableSettings(AccessToken.accessToken, homeAppliances[0].haid);
            var SettingList = await task3;

            //Console.WriteLine("Settings\n Key: "+ SettingList[0].key +"\nValue: "+ SettingList[0].value);
        }

        public static async Task<AccessToken> GetAccessToken()
        {
            AccessToken accessToken = new AccessToken();
            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.Resource = "/security/oauth/token";
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", string.Format("client_id={0}&redirect_uri={1}&grant_type=authorization_code&code=FB0655D295F2884B9D9DC9A37AF07D72E7031CAC258DAEDE7F06D0F7586F6F14", client_id, redirect_url), ParameterType.RequestBody);

                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    accessToken = JsonConvert.DeserializeObject<AccessToken>(response.Content);
                }
                else
                {
                    Debug.WriteLine("StatusCode: " + response.StatusCode);
                    Debug.WriteLine("StatusDescription: " + response.StatusDescription);
                }
                
                return accessToken;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return accessToken;
            }
        }

        public static async Task<List<HomeAppliance>> EnumerateHomeAppliances(string accessToken)
        {
            // Initialize empty HomeApplianceList
            List<HomeAppliance> homeAppliancesJsonList = new List<HomeAppliance>();

            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.Resource = "/api/homeappliances";
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Accept", content_type);
                request.AddHeader("Authorization", string.Format("Bearer {0}", accessToken));
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;

                var data = JsonConvert.DeserializeObject<Dictionary<string,Dictionary<string,object>>>(response.Content)["data"]["homeappliances"];

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    homeAppliancesJsonList = JsonConvert.DeserializeObject<List<HomeAppliance>>(data.ToString());
                }
                return homeAppliancesJsonList;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                
                return homeAppliancesJsonList;
            }
        }

        public static async Task<List<Status>> GetCurrentStatusOfHomeAppliance(string accessToken, string haid)
        {
            List<Status> status = new List<Status>();
            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.Resource = "/api/homeappliances/" + haid + "/status";
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Authorization", String.Format("Bearer {0}", accessToken));
                request.AddHeader("Accept", content_type);
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;
                                
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response.Content)["data"]["status"];
                    status = JsonConvert.DeserializeObject<List<Status>>(data.ToString());
                }
                else
                {
                    Debug.WriteLine(response.ErrorMessage);
                }
                return status;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
                return status;
            }
        }

        public static async Task<List<Setting>> GetAvailableSettings(string accesToken, string haid)
        {
            List<Setting> settingList = new List<Setting>();

            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.Resource = "/api/homeappliances/" + haid + "/settings";
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Authorization", String.Format("Bearer {0}",accesToken));
                request.AddHeader("Accept", content_type);
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;
                                               
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine(response.StatusCode);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response.Content)["data"]["settings"];
                    settingList = JsonConvert.DeserializeObject<List<Setting>>(data.ToString());
                }
                else
                {                    
                    Debug.WriteLine("Status Code: "+ response.StatusCode);
                }
                return settingList;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return settingList;
                throw;
            }
        }
    }
}
