﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace JDBYSJ.Data
{
    public class NewsChannel
    {        
        public string channelId { get; set; }
        public string name { get; set; }
    }

    public class ShowAPI_res_body_NewsChannel
    {
        public ObservableCollection<NewsChannel> channelList { get; set; }
        public string ret_code { get; set; }
        public string totalNum { get; set; }
    }

    public class ShowAPI_NewsChanelClass
    {
        public string showapi_res_code { get; set; }
        public string showapi_res_error { get; set; }
        public ShowAPI_res_body_NewsChannel showapi_res_body { get; set; }
    }

    public sealed class NewsChannelsDataSource
    {
        private static NewsChannelsDataSource _nesChannelsDataSource = new NewsChannelsDataSource();




        //函数：刷新频道列表
        public static async Task<ShowAPI_res_body_NewsChannel> RefreshNewsChannels()
        {
            if(await _nesChannelsDataSource.GetNewsChannelsData())
            {
                var newschannelmache = _nesChannelsDataSource.NewsChannelsResBody;
                return newschannelmache.First();
            }
            return new ShowAPI_res_body_NewsChannel();
        }


        //函数：获取Json数据并实例化为类
        private async Task<bool> GetNewsChannelsData()
        {
            ShowNewsChannelAPIURL channelurl = new ShowNewsChannelAPIURL();
            string apiUrl = channelurl.ToString();
            bool isOK = false;
            string JsonText = await MrOwl_JasonSerializerClass.GetJsonText(apiUrl);
            ShowAPI_NewsChanelClass res = MrOwl_JasonSerializerClass.DataContractJasonSerializer<ShowAPI_NewsChanelClass>(JsonText);
            if (res.showapi_res_code == "0")
            {
                isOK = true;
                if(this._channelsResBody.Count != 0)
                {
                    this._channelsResBody.Clear();
                }
                this._channelsResBody.Add(res.showapi_res_body);
            }
            else
            {
                MessageDialog errormsgdlg = new MessageDialog(res.showapi_res_error, "错误！");
                await errormsgdlg.ShowAsync();
            }
            return isOK;
        }
        //成员变量：NewsChannels的集合
        //搜索出的新闻数据集合
        private ObservableCollection<ShowAPI_res_body_NewsChannel> _channelsResBody= new ObservableCollection<ShowAPI_res_body_NewsChannel>();

        public ObservableCollection<ShowAPI_res_body_NewsChannel> NewsChannelsResBody
        { get { return this._channelsResBody; } }

    }

}
