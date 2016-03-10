using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace JDBYSJ.Data
{
    class MrOwl_JasonSerializerClass
    {
        /// <summary>
        /// 通过Http网络请求获取Json字符串
        /// </summary>
        /// <param name="urlStr">网络API的Url字符串</param>
        /// <returns>返回请求到的Json字符串</returns>
        public static async Task<string> GetJsonText(string urlStr)
        {
            string responseText = "";
            HttpClient hClient = new HttpClient();
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            Uri resourceuri;
            if (Uri.TryCreate(urlStr, UriKind.Absolute, out resourceuri))
            {
                try
                {
                    responseMessage = await hClient.GetAsync(resourceuri);
                    responseMessage.EnsureSuccessStatusCode();
                    responseText = await responseMessage.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("网络请求错误" + ex.Message);
                }
            }
            return responseText;
        }

        /// <summary>
        /// 将Json字符串实例化为类
        /// </summary>
        /// <typeparam name="T">需要实例化的对象类型</typeparam>
        /// <param name="resText">传入Json字符串</param>
        /// <returns>返回Json的实例化类的对象</returns>
        public static T DataContractJasonSerializer<T>(string resText)
        {
            var ds = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(resText));
            T result = (T)ds.ReadObject(ms);
            ms.Dispose();
            return result;
        }


        public static bool CheckNetWork()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
