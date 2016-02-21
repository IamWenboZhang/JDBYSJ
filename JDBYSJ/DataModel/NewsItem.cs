using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace JDBYSJ.Data
{
    public class JsonImage
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class JsonTag
    {
        public List<string> own { get; set; }
        public List<string> tag { get; set; }
    }

    public class NewsItem
    {
        public NewsItem()
        { }

        public string channelId { get; set; }                                   //新闻频道ID
        public string channelName { get; set; }                                 //新闻频道名称
        public string desc { get; set; }                                        //新闻简单描述
        public string html { get; set; }                                        //新闻正文Html字符串
        public ObservableCollection<JsonImage> imageurls { get; set; }          //新闻图片数组
        public string link { get; set; }                                        //新闻原文链接
        public string long_desc { get; set; }                                   //新闻详细描述
        public string nid { get; set; }                                         //新闻对应的外网id
        public string pubDate { get; set; }                                     //新闻日期
        public string sentiment_display { get; set; }                           //
        public string source { get; set; }                                      //新闻来源
        public ObservableCollection<JsonTag> tags { get; set; }                 //新闻标签
        public string title { get; set; }                                       //新闻标题

    }

    public class PageBean
    {
        public string allNum { get; set; }
        public string allPages { get; set; }
        public ObservableCollection<NewsItem> contentlist { get; set; }
        public string currentPage { get; set; }
        public string maxResult { get; set; }
    }

    public class ShowAPI_res_body
    {
        public PageBean pagebean { get; set; }
        public string ret_code { get; set; }
    }

    public class ShowAPI_NewsClass
    {
        public string showapi_res_code { get; set; }
        public string showapi_res_error { get; set; }
        public ShowAPI_res_body showapi_res_body { get; set; }
    }

    public sealed class NewsDataSource
    {
        private static NewsDataSource _newsDataSource = new NewsDataSource(); 

        public static async Task<ShowAPI_NewsClass> GetShowAPI_NewsClassAsync(string apiUrl)
        {
            await _newsDataSource.GetNewsDataAsync(apiUrl);
            // 对于小型数据集可接受简单线性搜索
            var matches = _newsDataSource.SocialNewsResClass;
            return matches.First();
        }

        private ObservableCollection<ShowAPI_NewsClass> _resClass = new ObservableCollection<ShowAPI_NewsClass>();

        public ObservableCollection<ShowAPI_NewsClass> SocialNewsResClass
        { get { return this._resClass; } }

        //获取Json字符串
        private async Task<string> GetJsonText(string urlStr)
        {
            string responseText = "";
            HttpClient hClient = new HttpClient();
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            Uri resourceuri;
            if(Uri.TryCreate(urlStr, UriKind.Absolute, out resourceuri))
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

        //Json转化为实体类
        public T DataContractJasonSerializer<T>(string resText)
        {
            var ds = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(resText));
            T result = (T)ds.ReadObject(ms);
            ms.Dispose();
            return result;
        }

        //获取新闻数据赋值给_resClass
        private async Task GetNewsDataAsync(string apiUrl)
        {
            if (this._resClass.Count != 0)
                return;

            string JsonText = await GetJsonText(apiUrl);
            ShowAPI_NewsClass res = DataContractJasonSerializer<ShowAPI_NewsClass>(JsonText);
            this._resClass.Add(res);
        }
    }
}
