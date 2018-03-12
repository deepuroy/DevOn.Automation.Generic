using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Automation.Api.Utilities
{
    public static class ServiceHandler
    {
        public static string ResponseBody;
        public static HttpStatusCode ResponseStatusCode;
        public static WebHeaderCollection ResponseHeaders;
        public static WebHeaderCollection RequestHeaders;
        public static Uri BaseUrl;
        public static WebProxy Proxy;

        public static void PostHttpRequest(this Uri serviceUrl, string requestBody, string contentType)
        {
            CreateHttpRequest(HttpMethod.Post, serviceUrl);
            Globals.Request.ContentLength = requestBody.Length;
            Globals.Request.ContentType = contentType;
            AddHttpRequestBody(requestBody);
            GetHttpResponse();
        }

        public static void GetHttpRequest(this Uri serviceUrl)
        {
            CreateHttpRequest(HttpMethod.Get, serviceUrl);
            GetHttpResponse();
        }

        public static void PutHttpRequest(this Uri serviceUrl, string requestBody)
        {
            CreateHttpRequest(HttpMethod.Put, serviceUrl);
            Globals.Request.ContentLength = requestBody.Length;
            Globals.Request.ContentType = "application/json";
            AddHttpRequestBody(requestBody);
            GetHttpResponse();
        }

        private static void AddHttpRequestBody(string body)
        {
            var buf = Encoding.UTF8.GetBytes(body);
            using (var dataStream = Globals.Request.GetRequestStream())
            {
                dataStream.Write(buf, 0, buf.Length);
                dataStream.Close();
            }
        }

        /// <summary>
        /// Creates the base Http request for the API test, also can set the request to go via proxy. This setting needs to be supplied from the test via Globals
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        private static void CreateHttpRequest(string method, Uri serviceUrl)
        {
            Globals.Request = (HttpWebRequest)WebRequest.Create(serviceUrl);
            if (Proxy != null)
            {
                Globals.Request.Proxy = Proxy;
                Globals.Request.PreAuthenticate = true;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
            Globals.Request.Method = method;
            Globals.Request.Headers.Add(RequestHeaders);
            Globals.Request.Accept = "*/*";
        }

        private static void GetHttpResponse()
        {
            try
            {
                using (Globals.Response = (HttpWebResponse)Globals.Request.GetResponse())
                {
                    ResponseStatusCode = Globals.Response.StatusCode;
                    ResponseHeaders = Globals.Response.Headers;
                    using (var data = Globals.Response.GetResponseStream())
                    {
                        if (data == null)
                        {
                            throw new Exception("No data in response.");
                        }
                        using (var reader = new StreamReader(data))
                        {
                            ResponseBody = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException e)
            {
                using (Globals.Response = (HttpWebResponse)e.Response)
                {
                    ResponseStatusCode = Globals.Response.StatusCode;
                    ResponseHeaders = Globals.Response.Headers;
                    using (var data = Globals.Response.GetResponseStream())
                    {
                        if (data == null)
                        {
                            throw new Exception("No data in response.");
                        }

                        using (var reader = new StreamReader(data))
                        {
                            ResponseBody = reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static bool VerifyResponseBody(IEnumerable<string> keysList)
        {
            var keysNotFound = keysList.Where(item => !ResponseBody.Contains(item)).ToList();
            return keysNotFound.Count <= 0;
        }

        public static bool VerifyResponseBody(Dictionary<string, string> keyValues)
        {
            try
            {
                var responseData = (JArray)JsonConvert.DeserializeObject(ResponseBody);
                if (responseData.Select(response => keyValues.All(keyValue => (response[keyValue.Key].ToString().Contains(keyValue.Value)))).Any(allKeysFound => allKeysFound))
                {
                    return true;
                }
            }
            catch (InvalidCastException)
            {
                var responseData = (JObject)JsonConvert.DeserializeObject(ResponseBody);
                return keyValues.All(keyValue => (responseData[keyValue.Key].ToString().Contains(keyValue.Value)));
            }

            return false;
        }

        /// <summary>
        /// This method will allow the tester to verify if response contains a specific key value pair set
        /// </summary>
        /// <param name="arrayName"></param>
        /// <param name="key"></param>
        /// <param name="expectedValue"></param>
        /// <returns></returns>
        public static bool VerifyKeyValueinArrayBodyRetrieved(this string ResponseBody, string arrayName, string key, string expectedValue)
        {
            var parsed = JObject.Parse(ResponseBody);
            var array = (JArray)parsed[arrayName];
            var resultItems = (from o in array.Children<JObject>() from p in o.Properties() where p.Name == key select p.Value.ToString()).ToList();
            return resultItems.Select(item => item.ToLower().Equals(expectedValue.ToLower())).All(itemHasKeyword => itemHasKeyword);
        }

        /// <summary>
        /// This is similar to a vlookup, this searches for a record which has a match for searchKey against supplied matchValue and return value of retrieveKey
        /// </summary>
        /// <param name="searchKey">Search Key</param>
        /// <param name="matchValue">Value to match with Search Key</param>
        /// <param name="retrieveKey">Retrieve Key</param>
        /// <returns></returns>
        public static string RetrieveKeyValueFromObject(string searchKey, string matchValue, string retrieveKey)
        {
            var rb = ResponseBody;
            rb = "{result:" + rb + "}";
            dynamic parsed = JObject.Parse(rb);
            JArray jArr = parsed.result;
            JObject jo = jArr.Children<JObject>().FirstOrDefault(o => o[searchKey].ToString().Contains(matchValue));
            return jo[retrieveKey].ToString();
        }

        public static string RetrieveKeyValue(string key)
        {
            var rb = ResponseBody;

            //When the response body is an array
            if (ResponseBody.Trim().StartsWith("["))
            {
                rb = "{result:" + rb + "}";
                dynamic parsed = JObject.Parse(rb);
                JArray jArr = parsed.result;
                return jArr[0][key].ToString();
            }
            //When the response body is a json object but encapsulated in braces
            else if (ResponseBody.Trim().StartsWith("{"))
            {
                rb = "{result:" + rb + "}";
                dynamic parsed = JObject.Parse(rb);
                JObject job = parsed.result;
                return job[key].ToString();
            }
            //When the response body is not escaped correctly and contains unwanted backslashes
            else if (ResponseBody.Trim().StartsWith("\"{"))
            {
                rb = "{result:" + rb.Replace("\"{", "{") + "}";
                rb = rb.Replace("}\"", "}");
                rb = rb.Replace(@"\", "");
                dynamic parsed = JObject.Parse(rb);
                JObject job = parsed.result;
                return job[key].ToString();
            }
            else if (ResponseBody.Trim().StartsWith(""))
            {
                return rb.ToString();
            }
            var parsed1 = JObject.Parse(rb);
            return parsed1[key].Value<string>();
        }
    }
}