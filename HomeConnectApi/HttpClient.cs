using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;


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
    void Start () {

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

}
