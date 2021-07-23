﻿using Middleware.Models.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Controllers
{
    public class AccountController
    {
        public static async Task<Models.OAuth> GetLoginToken(string username, string password)
        {
            var dict = new Dictionary<string, string>();
            string hashedPassword = ComputeSha256Hash(password);
            dict.Add("grant_type", "password");
            dict.Add("username",username);
            dict.Add("password", hashedPassword);
  

            using (HttpClient http = new HttpClient())
            {
                var data = new HttpRequestMessage(HttpMethod.Post, $"http://{Properties.DB_HOST}:8080/oauth/token");
                var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes("yugiohjwtclientid:XY7kmzoNzl100"));
                data.Headers.Add("Authorization", $"Basic {base64authorization}");
                

                data.Content = new FormUrlEncodedContent(dict);
                var response = await http.SendAsync(data).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) throw new Exception(await response.Content.ReadAsStringAsync());
                try
                {
                    return JsonConvert.DeserializeObject<Models.OAuth>(await response.Content.ReadAsStringAsync());
                }
                catch (JsonSerializationException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return null;
        }

        public static async Task<string> GetRegistrationCode(string username, string password)
        {
            var dict = new Dictionary<string, string>();
            string hashedPassword = ComputeSha256Hash(password);
            dict.Add("username", username);
            dict.Add("password", hashedPassword);


            using (HttpClient http = new HttpClient())
            {
                var data = new HttpRequestMessage(HttpMethod.Post, $"http://{Properties.DB_HOST}:8080/api/account/register");

                var json = JsonConvert.SerializeObject(dict);

                data.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await http.SendAsync(data).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) throw new Exception(await response.Content.ReadAsStringAsync());
                try
                {
                    return JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                }
                catch (JsonSerializationException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return null;
        }

        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
