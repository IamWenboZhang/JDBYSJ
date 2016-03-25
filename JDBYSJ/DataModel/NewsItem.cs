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
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Popups;

namespace JDBYSJ.Data
{
    public class JsonImage
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    //public class JsonTag
    //{
    //    public List<string> own { get; set; }
    //    public List<string> tag { get; set; }
    //}

    public class NewsItem
    {
        public NewsItem()
        {
            this.channelId = "";
            this.channelName = "";
            this.desc = "";
            this.html = "";
            this.imageurls = new ObservableCollection<JsonImage>();
            this.link = "";
            this.long_desc = "";
            this.nid = "";
            this.pubDate = "";
            this.sentiment_display = "";
            this.source = "";
            this.title = "";           
        }

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
        //public ObservableCollection<JsonTag> tags { get; set; }                 //新闻标签
        public string title { get; set; }                                       //新闻标题

    }

    public class PageBean
    {
        public PageBean()
        {
            this.allNum = "0";
            this.allPages = "0";
            this.contentlist = new ObservableCollection<NewsItem>();
            this.currentPage = "-1";
            this.maxResult = "0";
        }

        public string allNum { get; set; }
        public string allPages { get; set; }
        public ObservableCollection<NewsItem> contentlist { get; set; }
        public string currentPage { get; set; }
        public string maxResult { get; set; }
    }

    public class ShowAPI_res_body_NewsItem
    {
        public ShowAPI_res_body_NewsItem()
        {
            this.pagebean = new PageBean();
            this.ret_code = "";
        }

        public PageBean pagebean { get; set; }
        public string ret_code { get; set; }
    }

    public class ShowAPI_NewsClass
    {
        public ShowAPI_NewsClass()
        {
            this.showapi_res_body = new ShowAPI_res_body_NewsItem();
            this.showapi_res_code = "-1";
            this.showapi_res_error = "";
        }
        public string showapi_res_code { get; set; }
        public string showapi_res_error { get; set; }
        public ShowAPI_res_body_NewsItem showapi_res_body { get; set; }
    }

    public sealed class NewsDataSource
    {
        private static NewsDataSource _newsDataSource = new NewsDataSource();

        //搜索新闻
        public static async Task<ShowAPI_NewsClass> SearchNews(string mainword)
        {
            string apiUrl;
            if(_newsDataSource._searchresClass.Count != 0)
            {
                _newsDataSource._searchresClass.Clear();
            }
            ShowNewsAPIURL searchNewsAPIUrl = new ShowNewsAPIURL("109-35","", "", "", "1", mainword, "1");
            apiUrl = searchNewsAPIUrl.ToString();
            if(await _newsDataSource.GetNewsDataAsync(apiUrl, NewsChannelsType.Search))
            {
                ShowAPI_NewsClass tmp = new ShowAPI_NewsClass();
                var searchmatches = _newsDataSource.SearchNewsPages;
                return searchmatches.First();
            }
            else
            {
                var searchmatches = _newsDataSource.SearchNewsPages;
                MessageDialog errormsgdlg = new MessageDialog("请检查本机时间是否准确以及网络是否畅通。", "错误！");
                await errormsgdlg.ShowAsync();
                errormsgdlg = null;
                return new ShowAPI_NewsClass();
            }
        }

        //获取下一页新闻内容（供上拉加载使用）
        public static async Task<ShowAPI_NewsClass> GetNextPage(ShowAPI_NewsClass ChannelNewses, NewsChannelsType type,int currentpage)
        {
            string apiUrl;
            switch (type)
            {
                case NewsChannelsType.Social:
                    //判断是否是最后一页如果不是则根据CurrentPage加载下一页的新闻内容
                    if (currentpage < Convert.ToInt32(ChannelNewses.showapi_res_body.pagebean.allPages))
                    {
                        string nextpage = (currentpage + 1).ToString();
                        ShowNewsAPIURL socialNewsAPIUrl = new ShowNewsAPIURL("109-35",App.SoicalChannelID, "", "", "1", "", nextpage);
                        apiUrl = socialNewsAPIUrl.ToString();
                        if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                        {
                            var socialmatches = _newsDataSource.SocialNewsPages;                            
                            return socialmatches[Convert.ToInt32(nextpage) - 1];
                        }
                        else
                        {
                            return new ShowAPI_NewsClass();
                        }
                    }
                    else
                    {
                        var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                        await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            MessageDialog errormsgdlg = new MessageDialog("没有更多了！", "提示");
                            errormsgdlg.ShowAsync();
                            errormsgdlg = null;
                        });                                               
                    }
                    break;
                case NewsChannelsType.GuoJi:
                    if (currentpage < Convert.ToInt32(ChannelNewses.showapi_res_body.pagebean.allPages))
                    {
                        string nextpage = (currentpage + 1).ToString();
                        ShowNewsAPIURL yuleNewsAPIUrl = new ShowNewsAPIURL("109-35",App.GuoJiChannelID, "", "", "1", "", nextpage);
                        apiUrl = yuleNewsAPIUrl.ToString();                        
                        if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                        {
                            var yulematches = _newsDataSource.GuojiNewsPages;
                            return yulematches[Convert.ToInt32(nextpage) - 1];
                        }
                        else
                        {
                            return new ShowAPI_NewsClass();
                        }
                    }
                    else
                    {
                        var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                        await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            MessageDialog errormsgdlg = new MessageDialog("没有更多了！", "提示");
                            errormsgdlg.ShowAsync();
                            errormsgdlg = null;
                        });
                    }
                    break;
                case NewsChannelsType.Technology:
                    if (currentpage < Convert.ToInt32(ChannelNewses.showapi_res_body.pagebean.allPages))
                    {
                        string nextpage = (currentpage + 1).ToString();
                        ShowNewsAPIURL technologyNewsAPIUrl = new ShowNewsAPIURL("109-35",App.TechnologyChannelID, "", "", "1", "", nextpage);
                        apiUrl = technologyNewsAPIUrl.ToString();                       
                        if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                        {
                            var technologymatches = _newsDataSource.TechnologyNewsPages;
                            return technologymatches[Convert.ToInt32(nextpage) - 1];
                        }
                        else
                        {
                            return new ShowAPI_NewsClass();
                        }
                    }
                    else
                    {
                        var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                        await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            MessageDialog errormsgdlg = new MessageDialog("没有更多了！", "提示");
                            errormsgdlg.ShowAsync();
                            errormsgdlg = null;
                        });
                    }
                    break;
                case NewsChannelsType.Self:
                    if (currentpage < Convert.ToInt32(ChannelNewses.showapi_res_body.pagebean.allPages))
                    {
                        string nextpage = (currentpage + 1).ToString();
                        ShowNewsAPIURL searcNewsAPIUrl = new ShowNewsAPIURL("109-35","", "", "", "1", "", nextpage);
                        apiUrl = searcNewsAPIUrl.ToString();
                        if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                        {
                            var selfmatches = _newsDataSource.SelfNewsPges;
                            return selfmatches[Convert.ToInt32(nextpage) - 1];
                        }
                        else
                        {
                            return new ShowAPI_NewsClass();
                        }
                    }
                    else
                    {
                        var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                        await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            MessageDialog errormsgdlg = new MessageDialog("没有更多了！", "提示");
                            errormsgdlg.ShowAsync();
                            errormsgdlg = null;
                        });
                    }
                    break;
                case NewsChannelsType.Search:
                    if (currentpage < Convert.ToInt32(ChannelNewses.showapi_res_body.pagebean.allPages))
                    {
                        string nextpage = (currentpage + 1).ToString();
                        ShowNewsAPIURL searchNewsAPIUrl = new ShowNewsAPIURL("109-35", "", "", "", "1", SearchPage.MainWord, nextpage);
                        apiUrl = searchNewsAPIUrl.ToString();
                        if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                        {
                            var serachmatches = _newsDataSource.SearchNewsPages;
                            return serachmatches[Convert.ToInt32(nextpage) - 1];
                        }
                        else
                        {
                            return new ShowAPI_NewsClass();
                        }
                    }
                    else
                    {
                        var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                        await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            MessageDialog errormsgdlg = new MessageDialog("没有更多了！", "提示");
                            errormsgdlg.ShowAsync();
                            errormsgdlg = null;
                        });
                    }
                    break;
            }
            return new ShowAPI_NewsClass ();
        }

        //获取第一页新闻内容（供下拉刷新使用）
        public static async Task<ShowAPI_NewsClass> GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType type)
        {
            string apiUrl = String.Empty;
            switch (type)
            {
                case NewsChannelsType.Social:
                    if (_newsDataSource._socialresClass.Count != 0)
                    {
                        _newsDataSource._socialresClass.Clear();
                    }
                    ShowNewsAPIURL socialNewsAPIUrl = new ShowNewsAPIURL("109-35",App.SoicalChannelID, "", "", "1", "", "1");
                    apiUrl = socialNewsAPIUrl.ToString();                    
                   
                    if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                    {
                        var socialmatches = _newsDataSource.SocialNewsPages;
                        return socialmatches.First();
                    }
                    else
                    {
                        return new ShowAPI_NewsClass();
                    }
                case NewsChannelsType.GuoJi:
                    if (_newsDataSource._guojiresClass.Count != 0)
                    {
                        _newsDataSource._guojiresClass.Clear();
                    }
                    ShowNewsAPIURL guojiNewsAPIUrl = new ShowNewsAPIURL("109-35",App.GuoJiChannelID, "", "", "1", "", "1");
                    apiUrl = guojiNewsAPIUrl.ToString();                   
                   
                    if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                    {
                        var guojimatches = _newsDataSource.GuojiNewsPages;
                        return guojimatches.First();
                    }
                    else
                    {
                        return new ShowAPI_NewsClass();
                    }
                case NewsChannelsType.Technology:
                    if (_newsDataSource._technologyresClass.Count != 0)
                    {
                        _newsDataSource._technologyresClass.Clear();
                    }
                    ShowNewsAPIURL technologyNewsAPIUrl = new ShowNewsAPIURL("109-35",App.TechnologyChannelID, "", "", "1", "", "1");
                    apiUrl = technologyNewsAPIUrl.ToString();
                    
                    
                    if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                    {
                        var technologymatches = _newsDataSource.TechnologyNewsPages;
                        return technologymatches.First();
                    }
                    else
                    {
                        return new ShowAPI_NewsClass();
                    }
                case NewsChannelsType.Self:
                    if (_newsDataSource._selfresClass.Count != 0)
                    {
                        _newsDataSource._selfresClass.Clear();
                    }
                    ShowNewsAPIURL selfNewsAPIUrl = new ShowNewsAPIURL("109-35",App.SelfChannelID, "", "", "1", "", "1");
                    apiUrl = selfNewsAPIUrl.ToString();
                    
                    
                    if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                    {
                        var selfmatches = _newsDataSource.SelfNewsPges;
                        return selfmatches.First();
                    }
                    else
                    {
                        return new ShowAPI_NewsClass();
                    }
                case NewsChannelsType.Search:
                    if(SearchPage.MainWord.Length != 0)
                    {
                        if (_newsDataSource.SearchNewsPages.Count != 0)
                        {
                            _newsDataSource._searchresClass.Clear();
                        }
                        ShowNewsAPIURL searchNewsAPIUrl = new ShowNewsAPIURL("109-35", "", "", "", "1", SearchPage.MainWord, "1");
                        apiUrl = searchNewsAPIUrl.ToString();
                        if (await _newsDataSource.GetNewsDataAsync(apiUrl, type))
                        {
                            var searchmatches = _newsDataSource.SearchNewsPages;
                            return searchmatches.First();
                        }
                        else
                        {
                            return new ShowAPI_NewsClass();
                        }
                    }
                    else
                    {
                        return new ShowAPI_NewsClass();
                    }
            }
            return new ShowAPI_NewsClass();
        }

        //搜索出的新闻数据集合
        private ObservableCollection<ShowAPI_NewsClass> _searchresClass = new ObservableCollection<ShowAPI_NewsClass>();

        public ObservableCollection<ShowAPI_NewsClass> SearchNewsPages
        { get { return this._searchresClass; } }

        //社会最新新闻的集合
        private ObservableCollection<ShowAPI_NewsClass> _socialresClass = new ObservableCollection<ShowAPI_NewsClass>();

        public ObservableCollection<ShowAPI_NewsClass> SocialNewsPages
        { get { return this._socialresClass; } }

        //娱乐焦点新闻的集合
        private ObservableCollection<ShowAPI_NewsClass> _guojiresClass = new ObservableCollection<ShowAPI_NewsClass>();

        public ObservableCollection<ShowAPI_NewsClass> GuojiNewsPages
        { get { return this._guojiresClass; } }

        //科技最新新闻的集合
        private ObservableCollection<ShowAPI_NewsClass> _technologyresClass = new ObservableCollection<ShowAPI_NewsClass>();

        public ObservableCollection<ShowAPI_NewsClass> TechnologyNewsPages
        { get { return this._technologyresClass; } }

        //私人订阅的新闻集合
        private ObservableCollection<ShowAPI_NewsClass> _selfresClass = new ObservableCollection<ShowAPI_NewsClass>();

        public ObservableCollection<ShowAPI_NewsClass> SelfNewsPges
        { get { return this._selfresClass; } }



        

        public static async Task<NewsItem> GetItemAsync(string link, string type)
        {
            switch(type)
            {
                case "0":
                    var socialmatches = _newsDataSource.SocialNewsPages.SelectMany(social => social.showapi_res_body.pagebean.contentlist).Where((item) => item.link.Equals(link));
                    if (socialmatches.Count() != 0) return socialmatches.First();
                    break;
                case "1":
                    var guojimatches = _newsDataSource.GuojiNewsPages.SelectMany(yule => yule.showapi_res_body.pagebean.contentlist).Where((item) => item.link.Equals(link));
                    if (guojimatches.Count() != 0) return guojimatches.First();
                    break;
                case "2":
                    var technologymatches = _newsDataSource.TechnologyNewsPages.SelectMany(tech => tech.showapi_res_body.pagebean.contentlist).Where((item) => item.link.Equals(link));
                    if (technologymatches.Count() != 0) return technologymatches.First();
                    break;
                case "3":
                    if(App.SelfChannelID.Length != 0)
                    {
                        var selfmatches = _newsDataSource.SelfNewsPges.SelectMany(self => self.showapi_res_body.pagebean.contentlist).Where((item) => item.link.Equals(link));
                        if (selfmatches.Count() != 0) return selfmatches.First();
                    }
                    break;
                case "-1":
                    var searchmatches = _newsDataSource.SearchNewsPages.SelectMany(search => search.showapi_res_body.pagebean.contentlist).Where((item) => item.link.Equals(link));
                    if (searchmatches.Count() != 0) return searchmatches.First();
                    break;
            }
            return new NewsItem();
        }     

        //获取新闻数据赋值给对应频道的集合
        public async Task<bool> GetNewsDataAsync(string apiUrl,NewsChannelsType type)
        {
            bool isOK = false;
            if (MrOwl_JasonSerializerClass.CheckNetWork())
            {
                string JsonText = await MrOwl_JasonSerializerClass.GetJsonText(apiUrl);
                if(JsonText != null)
                {
                    ShowAPI_NewsClass res = MrOwl_JasonSerializerClass.DataContractJasonSerializer<ShowAPI_NewsClass>(JsonText);
                    if (res.showapi_res_code == "0")
                    {
                        for (int i = 0; i < res.showapi_res_body.pagebean.contentlist.Count; i++)
                        {
                            if (res.showapi_res_body.pagebean.contentlist[i].imageurls.Count == 0)
                            {
                                JsonImage img = new JsonImage();
                                img.url = "ms-appx:///Assets/zhuanshu150.png";
                                res.showapi_res_body.pagebean.contentlist[i].imageurls.Add(img);
                            }
                            if (res.showapi_res_body.pagebean.contentlist[i].html.Length == 0)
                            {
                                res.showapi_res_body.pagebean.contentlist.Remove(res.showapi_res_body.pagebean.contentlist[i]);
                                i--;
                            }
                        }
                        switch (type)
                        {
                            case NewsChannelsType.Social:
                                this._socialresClass.Add(res);
                                isOK = true;
                                return isOK; ;
                            case NewsChannelsType.GuoJi:
                                this._guojiresClass.Add(res);
                                isOK = true;
                                return isOK; ;
                            case NewsChannelsType.Technology:
                                this._technologyresClass.Add(res);
                                isOK = true;
                                return isOK; ;
                            case NewsChannelsType.Self:
                                this._selfresClass.Add(res);
                                isOK = true;
                                return isOK; ;
                            case NewsChannelsType.Search:
                                this._searchresClass.Add(res);
                                isOK = true;
                                return isOK; ;
                        }
                    }
               
                }
                else
                {
                    //MessageDialog errormsgdlg = new MessageDialog(res.showapi_res_error, "错误！");
                    //await errormsgdlg.ShowAsync();
                    //errormsgdlg = null;
                }
            }
            else
            {
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    MessageDialog errormsgdlg = new MessageDialog("请检查你的网络连接", "错误！");
                    errormsgdlg.ShowAsync();
                    errormsgdlg = null;
                });
            }            
            return isOK;
        }
    }
}
