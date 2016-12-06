using System;
using System.Web;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public Setting(string _key, string _value)
        {
            key = _key;
            value = _value;
        }
    }

    public class Program
    {
        [JsonProperty("key")]
        public string name { get; set; }
        [JsonProperty("options")]
        public List<ProgramOptions> programOptions { get; set; }
        
        public Program(string _name)
        {
            name = _name;
        }

    }
    public class NameOfProgram
    {   
        [JsonProperty("key")]
        public string name { get; set; }
    }
    public class ProgramOptions
    {
        [JsonProperty("key")]
        public string key { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("value")]
        public int value { get; set; }
        [JsonProperty("unit")]
        public string unit { get; set; }
        public ProgrammConstraints constraints { get; set; } 
    }
    public class ProgrammConstraints
    {
        [JsonProperty("min")]
        public int min { get; set; }
        [JsonProperty("max")]
        public int max { get; set; }
    }

    class HomeConnect
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
            Console.WriteLine("AccessToken = " + AccessToken.accessToken + "\nScope: " + AccessToken.scope);
            string idAccessToken = AccessToken.accessToken;

            // Save the connected HomeAppliances 
            var task1 = EnumerateHomeAppliances(AccessToken.accessToken);
            var homeAppliances = await task1;
            foreach (var appliance in homeAppliances)
            {
                Console.WriteLine(appliance.type);
            }
            string stringHaidOven = homeAppliances[4].haid;

            //var task2 = GetCurrentStatusOfHomeAppliance(AccessToken.accessToken, homeAppliances[0].haid);
            //var StatusList = await task2;

            // Get available settings of oven 
            var task4 = GetAvailableSettings(AccessToken.accessToken, homeAppliances[3].haid);
            var settings = await task4;
            foreach (var setting in settings)
            {
                //Console.WriteLine("Before the set call");
                //Console.WriteLine("key: " + setting.key + "\nvalue: " + setting.value);
            }

            /*
            var task5 = GetExecutedProgram(AccessToken.accessToken, homeAppliances[4].haid);
            var result = await task5;
            Console.WriteLine(result.name);
            foreach (var option in result.programOptions)
            {
                Console.WriteLine("key: " + option.key + "\nvalue: " + option.value + "\nunit: " + option.unit);
            }*/

            var task7 = GetAllAvailablePrograms(idAccessToken, stringHaidOven);
            var availableProgramNames = await task7;
            foreach (var programName in availableProgramNames)
            {
                Console.WriteLine(programName.name);
            }

            var task8 = GetSpecificAvailableProgram(idAccessToken, stringHaidOven, availableProgramNames[0].name);

        }
        // ACCESS TOKEN
        /// <summary>
        /// Get Access Token
        /// </summary>
        /// <returns>AccessToken Object</returns>
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
        /// <summary>
        /// After expiration of AccessToken, one has to refresh the Aceess Token 
        /// <param name="refreshToken">The generated Refresh Token from GetAccessToken()</param>
        /// </summary>
        /// <returns>AccessToken Object</returns>
        public static async Task<AccessToken> RefreshAccessToken(string refreshToken)
        {
            AccessToken newAccessToken = new AccessToken();

            try
            {
                var client = new RestClient(baseUrl);                
                var request = new RestRequest(Method.POST);
                request.Resource = "/security/oauth/token";
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", String.Format("grant_type=refresh_token&refresh_token={0}",refreshToken), ParameterType.RequestBody);
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    newAccessToken = JsonConvert.DeserializeObject<AccessToken>(response.Content);
                }
                return newAccessToken;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
                return newAccessToken;
            }
            
        }
        // ENUMERATE HOME APPLIANCES
        /// <summary>
        /// Get a list of all the connected HomeAppliances        
        /// </summary>
        /// <param name="accessToken">Generated AccessToken</param>
        /// <returns>A list of HomeAppliance Objects</returns>
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
        /// <summary>
        /// Get current status of one specific home appliance.
        /// Usually a key<>value pair of information about the status itself and its value
        /// Sometimes one has a unit, e.g. temperature of the oven
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="haid">Home Appliance Identifier</param>
        /// <returns>A list of Status Objects</returns>
        // STATUS
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
        /// <summary>
        /// Get all programs which are currently available on the given home appliance
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="haid"></param>
        /// <returns></returns>
        // PROGRAMS
        public static async Task<List<NameOfProgram>> GetAllAvailablePrograms(string accessToken, string haid)
        {
            List<NameOfProgram> nameOfProgram = new List<NameOfProgram>();

            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.Resource = "/api/homeappliances/" + haid + "/programs/available";
                request.AddHeader("Authorization", String.Format("Bearer {0}", accessToken));
                request.AddHeader("Accept", content_type);
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {                    
                    var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response.Content)["data"]["programs"];
                    nameOfProgram = JsonConvert.DeserializeObject<List<NameOfProgram>>(data.ToString());
                    return nameOfProgram;                    
                }
                else
                {
                    Console.WriteLine(response.Content);
                    return nameOfProgram;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return nameOfProgram;
                throw e;
            }
        }
        /// <summary>
        /// Get specific available program
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="haid"></param>
        /// <param name="programKey">Key of the program</param>
        /// <returns>Program Object</returns>
        public static async Task<Program> GetSpecificAvailableProgram(string accessToken, string haid, string programKey)
        {
            Program program = new Program(programKey);

            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.Resource = String.Format("/api/homeappliances/{0}/programs/available/{1}", haid, programKey);
                request.AddHeader("Accept", content_type);
                request.AddHeader("Authorization", String.Format("Bearer {0}", accessToken));
                var response = await client.ExecuteTaskAsync(request) as RestResponse;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var options = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content)["data"];
                    program = JsonConvert.DeserializeObject<Program>(options.ToString());
                                        
                    return program;
                }
                else
                {
                    Console.WriteLine(response.Content);
                    return program;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return program;
            }
        }
        /// <summary>
        /// Get program which is currently executed
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="haid"></param>
        /// <returns>Program Object containing a list of all the options</returns>
        public static async Task<Program> GetExecutedProgram(string accessToken, string haid)
        {
            Program program = new Program("No program running");
            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.Resource = string.Format("/api/homeappliances/{0}/programs/active", haid);
                request.AddHeader("Authorization", string.Format("Bearer {0}", accessToken));
                request.AddHeader("Accept", content_type);
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content)["data"];
                    var name = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString())["key"];
                    var options = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString())["options"];
                    List<ProgramOptions> programOptions = new List<ProgramOptions>();
                    programOptions = JsonConvert.DeserializeObject<List<ProgramOptions>>(options.ToString());
                    program.name = name.ToString();
                    program.programOptions = programOptions;
                    return program;
                }
                else
                {
                    Console.WriteLine(response.Content);
                    return program;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return program;
            }
        }
        /// <summary>
        /// Get the program which is currently selected
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="haid"></param>
        /// <returns>Program Object containing a list of all the options</returns>
        public static async Task<Program> GetSelectedProgram(string accessToken, string haid)
        {
            Program program = new Program("No selected program");

            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.Resource = string.Format("/api/homeappliances/{0}/programs/selected", haid);
                request.AddHeader("Authorization", string.Format("Bearer {0}", accessToken));
                request.AddHeader("Accept", content_type);
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content)["data"];
                    var name = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString())["key"];
                    var options = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString())["options"];
                    List<ProgramOptions> programOptions = new List<ProgramOptions>();
                    programOptions = JsonConvert.DeserializeObject<List<ProgramOptions>>(options.ToString());
                    program.name = name.ToString();
                    program.programOptions = programOptions;
                    return program;
                }
                else
                {
                    Debug.WriteLine(response.StatusDescription);
                    return program;
                }
            }
            catch (Exception e)
            {
                return program;
                throw e;
            }
        }
        /// <summary>
        /// Stop the program which is currently executed
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="haid"></param>
        /// <returns>Message, either good or bad</returns>
        public static async Task StopExecutedProgram(string accessToken, string haid)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest(Method.DELETE);
            request.Resource = string.Format("/api/homeappliances/{0}/programs/active", haid);
            request.AddHeader("Authorization", string.Format("Bearer {0}", accessToken));
            request.AddHeader("Accept", content_type);
            RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;
            if ((int)response.StatusCode == 204)
            {
                Console.WriteLine("Program stopped successfully");
            }
            else
            {
                Console.WriteLine(response.Content);
            }
        }
        // SETTINGS
        /// <summary>
        /// Get a list of available settings
        /// </summary>
        /// <param name="accesToken"></param>
        /// <param name="haid">HomeApplianceIdentifier</param>
        /// <returns>A list of Setting Objects</returns>        
        public static async Task<List<Setting>> GetAvailableSettings(string accesToken, string haid)
        {
            List<Setting> settingList = new List<Setting>();

            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.GET);
                request.Resource = "/api/homeappliances/" + haid + "/settings";                
                request.AddHeader("Authorization", String.Format("Bearer {0}",accesToken));
                request.AddHeader("Accept", content_type);
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;
                                               
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {                    
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
            }
        }
        /// <summary>
        /// Set a specific setting for a given HomeAppliance
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="haid"></param>
        /// <param name="settingKey">Setting Key has to be provided</param>
        /// <returns>StatusCode</returns>
        public static async Task SetSpecificSetting(string accessToken, string haid, string settingKey, bool settingValue)
        {
            try
            {                
                var client = new RestClient(baseUrl);                
                var request = new RestRequest(Method.PUT);
                request.Resource = String.Format("/api/homeappliances/{0}/settings/{1}",haid,settingKey);
                request.AddHeader("Authorization", string.Format("Bearer {0}",accessToken));
                request.AddHeader("Accept", content_type);
                request.AddHeader("Content-Type", content_type);
                // Request Body
                var body = JsonConvert.SerializeObject(new { key = settingKey, value = settingValue });
                var data = JsonConvert.SerializeObject(new { data = body });
                request.Parameters.Clear();
                request.AddParameter(content_type, data, ParameterType.RequestBody);
                
                RestResponse response = await client.ExecuteTaskAsync(request) as RestResponse;
                Console.WriteLine((int)response.StatusCode);
                Console.WriteLine(response.Content);
                if ((int)response.StatusCode == 204)
                {
                    Console.WriteLine("Setting updated successfully");
                }
                else
                {
                    Console.WriteLine("An error occured");
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
        
    }
}
