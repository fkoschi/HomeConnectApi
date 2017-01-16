using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

public class HomeConnectHttpClient
{
    /// <summary>
    /// oObject - Prefix for Object
    /// lObject - Prefix for a list of Objects
    /// </summary>

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

    public class Program
    {
        [JsonProperty("key")]
        public string name { get; set; }
        [JsonProperty("options")]
        public List<ProgramOptions> programOptions { get; set; }
        public Program() { }
        public Program(string _name)
        {
            name = _name;
        }
    }

    public class ProgramOptions
    {
        [JsonProperty("key")]
        public string key { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("value")]
        public object value { get; set; }
        [JsonProperty("unit")]
        public string unit { get; set; }
        public ProgramConstraints constraints { get; set; }
    }

    public class ProgramConstraints
    {
        [JsonProperty("min")]
        public int min { get; set; }
        [JsonProperty("max")]
        public int max { get; set; }
    }

    public class Setting
    {
        [JsonProperty("key")]
        public string key { get; set; }
        [JsonProperty("value")]
        public string value { get; set; }
        [JsonProperty("unit")]
        public string unit { get; set; }
        // empty constructor
        public Setting() { }
        public Setting(string _key, string _value)
        {
            key = _key;
            value = _value;
        }
    }

    public class Event
    {
        public string eventName { get; set; }
        public List<EventData> eventData { get; set; }
        public string haid { get; set; }
    }

    public class EventData
    {
        public string key { get; set; }
        public int timestamp { get; set; }
        public string level { get; set; }
        public string handling { get; set; }
        public int value { get; set; }
        public string unit { get; set; }
    }
    
    // AccessToken 
    static public AccessToken oAccessToken = new AccessToken();
    // Home Appliances
    static public HomeAppliance Oven = new HomeAppliance();
    static public HomeAppliance Dishwasher = new HomeAppliance();
    static public HomeAppliance FridgeFreezer = new HomeAppliance();
    static public HomeAppliance CoffeeMaker = new HomeAppliance();
    static public HomeAppliance Dryer = new HomeAppliance();
    static public HomeAppliance Washer = new HomeAppliance();

    // Base URL 
    static Uri baseUri = new Uri("https://developer.home-connect.com");
    // Client_ID
    static private string client_id = "434AB33977AF19E6B34F7C4FFE7983BF2D7826D4872E8F56AD0782DEAEED2DB2";
    // Redirect URL 
    static private string redirect_url = "https://apiclient.home-connect.com/o2c.html";
    // Content-Type
    static private string content_type = "application/vnd.bsh.sdk.v1+json";
    // Code 
    static private string code = "FB0655D295F2884B9D9DC9A37AF07D72E7031CAC258DAEDE7F06D0F7586F6F14";
    


    // Use this for initialization
    static void Main () {
        Console.WriteLine("Hello Main");
        // Initialize accessToken as Object
        oAccessToken = getAccessToken().Result;
        // AccessToken available
        if (oAccessToken != null)
        {
            // Get all connected home appliances 
            var lHomeAppliances = EnumerateHomeAppliances(oAccessToken.accessToken).Result;
            // Assign one appliance to the related object
            if (lHomeAppliances.Count > 0)
            {
                foreach (var oHomeAppliance in lHomeAppliances)
                {
                    if (oHomeAppliance.type == "Oven")
                    {
                        Oven = oHomeAppliance;
                    }
                    else if (oHomeAppliance.type == "Dishwasher")
                    {
                        Dishwasher = oHomeAppliance;
                    }
                    else if (oHomeAppliance.type == "FridgeFreezer")
                    {
                        FridgeFreezer = oHomeAppliance;
                    }
                    else if (oHomeAppliance.type == "CoffeeMaker")
                    {
                        CoffeeMaker = oHomeAppliance;
                    }
                }
            }

            Program program = new Program();            
            program.name = "Cooking.Oven.Program.HeatingMode.PizzaSetting";
            ProgramOptions programOption1 = new ProgramOptions();
            programOption1.key = "Cooking.Oven.Option.SetpointTemperature";
            programOption1.value = 230;
            programOption1.unit = "°C";
            ProgramOptions programOption2 = new ProgramOptions();
            programOption2.key = "BSH.Common.Option.Duration";
            programOption2.value = 1200;
            programOption2.unit = "seconds";

            List<ProgramOptions> programOptions = new List<ProgramOptions>();
            programOptions.Add(programOption1);
            programOptions.Add(programOption2);

            program.programOptions = programOptions;

            var data = JsonConvert.SerializeObject(new { data = program }, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            string body = data.ToString();

            var result = StartGivenProgram(oAccessToken.accessToken, Oven.haid, body);
            
            Console.ReadLine();
        }        
    }

    // Update is called once per frame
    void Update () {
	
	}
    
    /// <summary>
    /// Get Access Token
    /// </summary>
    /// <returns>AccessToken Object</returns>
    public static async Task<AccessToken> getAccessToken()
    {
        AccessToken accessToken = new AccessToken();
        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = baseUri;
            var parameters = new Dictionary<string, string> { { "client_id", client_id }, { "redirect_uri", redirect_url }, { "grant_type", "authorization_code" }, { "code", code } };
            var encodedContent = new FormUrlEncodedContent(parameters);

            var response = await client.PostAsync("/security/oauth/token", encodedContent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                accessToken = JsonConvert.DeserializeObject<AccessToken>(content);                
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Nothing found ... ");
            }
            return accessToken;
        }
    }
    /// <summary>
    /// Enumerate the connected Home Appliances 
    /// </summary>
    /// <param name="accessToken">The mandatory accessToken to perform the method call.</param>
    /// <returns>Either one or a list of HomeAppliance Objects.</returns>
    public static async Task<List<HomeAppliance>> EnumerateHomeAppliances(string accessToken)
    {
        List<HomeAppliance> homeAppliances = new List<HomeAppliance>();

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content_type));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("/api/homeappliances");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(content)["data"]["homeappliances"];
                homeAppliances = JsonConvert.DeserializeObject<List<HomeAppliance>>(data.ToString());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(response.Content);
            }
            return homeAppliances;
        }

    }
    /// <summary>
    /// Get a new accessToken through the refreshToken of the existing Token and assign it to the existing one
    /// </summary>
    /// <param name="refreshToken">The refreshToken of the actual AccessToken</param>    
    public static async Task RefreshAccessToken(string refreshToken)
    {
        AccessToken refreshedAccessToken = new AccessToken();

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content_type));

            var parameter = new Dictionary<string, string> { { "grant_type", "refresh_token" }, { "refresh_token", refreshToken } };
            var encodedContent = new FormUrlEncodedContent(parameter);

            var response = await client.PostAsync("/security/oauth/token", encodedContent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                refreshedAccessToken = JsonConvert.DeserializeObject<AccessToken>(content);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(response.Content);
            }
            oAccessToken = refreshedAccessToken;
        }
    }
    /// <summary>
    /// Retrieve the current statuf of a home appliance
    /// </summary>
    /// <param name="accessToken">AccessToken</param>
    /// <param name="haid">HomeApplianceID</param>
    /// <returns>A list of Status Objects</returns>
    public static async Task<List<Status>> GetCurrentStatusOfHomeAppliance(string accessToken, string haid)
    {
        List<Status> lStatus = new List<Status>();

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content_type));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("/api/homeappliances/" + haid + "/status");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(content)["data"]["status"];
                lStatus = JsonConvert.DeserializeObject<List<Status>>(data.ToString());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Nothing found ... ");
            }
            return lStatus;
        }
    }
    /// <summary>
    /// Get some specific status information about one home appliance
    /// </summary>
    /// <param name="accessToken">AccessToken</param>
    /// <param name="haid">HomeApplianceID</param>
    /// <param name="statusKey">Key for the Status</param>
    /// <returns></returns>
    public static async Task<Status> GetSpecificCurrentStatusOfHomeAppliance(string accessToken, string haid, string statusKey)
    {
        Status status = new Status();
        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content_type));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("/api/homeappliances/" + haid + "/status/" + statusKey);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content)["data"];
                status = JsonConvert.DeserializeObject<Status>(data.ToString());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Nothing found ... ");
            }
            return status;
        }
    }
    /// <summary>
    /// Get the program which is currently selected
    /// </summary>
    /// <param name="accessToken">AccessToken</param>
    /// <param name="haid">HomeApplianceID</param>
    /// <returns>Program Object containing a list of all the options</returns>
    public static async Task<Program> GetSelectedProgram(string accessToken, string haid)
    {
        Program program = new Program("No selected program");
        using (HttpClient client = new HttpClient())
        {
            try
            {
                client.BaseAddress = baseUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content_type));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync("/api/homeappliances/" + haid + "/programs/selected");
                
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync();                    
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content)["data"];
                    var name = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString())["key"];
                    var options = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString())["options"];
                    List<ProgramOptions> programOptions = new List<ProgramOptions>();
                    programOptions = JsonConvert.DeserializeObject<List<ProgramOptions>>(options.ToString());
                    program.name = name.ToString();
                    program.programOptions = programOptions;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Nothing found ... ");
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            
            return program;
        }           
    }
    /// <summary>
    /// Get specific setting
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="haid"></param>
    /// <param name="settingKey"></param>
    /// <returns>Object containing the key and value of the setting</returns>
    public static async Task<Setting> GetSpecificSetting(string accessToken, string haid, string settingKey)
    {
        Setting setting = new Setting();

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content_type));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("/api/homeappliances/" + haid + "/settings/" + settingKey);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content)["data"];
                setting = JsonConvert.DeserializeObject<Setting>(data.ToString());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Nothing found ... ");
            }
            return setting;
        }
    }
    /// <summary>
    /// Start a given program 
    /// options have to be delivered as a function parameter
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="haid"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static async Task StartGivenProgram(string accessToken, string haid, string body)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                client.BaseAddress = baseUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content_type));                                
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpContent httpcontent = new StringContent(body);
                httpcontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(content_type);

                HttpResponseMessage rpmsg = (await client.PutAsync("/api/homeappliances/" + haid + "/programs/active", httpcontent));
                if(rpmsg.IsSuccessStatusCode)
                {
                    Console.WriteLine(rpmsg.StatusCode);
                }
                else
                {
                    Console.WriteLine(rpmsg.Content.ReadAsStringAsync().Result);
                }                
            }
            catch (Exception e)
            {
                throw e;                
            }
        }
    }
    /// <summary>
    /// Consume all events on appliance
    /// There are various types of events happening, NOTIFIY etc.
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="haid"></param>
    /// <returns>No return value</returns>
    public async Task GetStreamOfEvents(string accessToken, string haid)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Infinite streaming of data
                client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

                client.BaseAddress = baseUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var request = new HttpRequestMessage(HttpMethod.Get, "/api/homeappliances/" + haid + "/events");
                using (var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead))
                {

                    using (var body = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(body))
                        while (!reader.EndOfStream)
                        {
                            string data = reader.ReadLine();

                            if (data.Contains("event:"))
                            {
                                string[] split = Regex.Split(data, "event: ");
                                string splitEventName = split[1];
                            }
                            if (data.Contains("data:"))
                            {
                                List<EventData> listEventData = new List<EventData>();
                                string[] split = Regex.Split(data, "data: ");
                                string jsonString = split[1];
                                var items = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString)["items"];
                                listEventData = JsonConvert.DeserializeObject<List<EventData>>(items.ToString());
                                foreach (var ev in listEventData)
                                {
                                }
                            }
                            if (data.Contains("id:"))
                            {
                                string[] split = Regex.Split(data, "id: ");
                                string splitId = split[1];
                            }
                        }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
