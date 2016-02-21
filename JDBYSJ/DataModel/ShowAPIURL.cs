using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JDBYSJ.Data
{
    class ShowAPIURL
    {
        public string showapi_appid = "13559";                                          //Show_API程序ID
        public string showapi_sign = "3124acc7667b477081b5854e9ab475f5";                //Show_API程序密钥
        public string showapi_timestamp = "";           //请求数据的时间
        public string channelId = "";                   //新闻频道的ID
        public string channelName = "";                 //新闻频道的名称
        public string needContent = "";                 //是否需要正文
        public string needHtml = "";                    //是否需要正文的Html字符串
        public string title = "";                       //查询的新闻标题
        public string page = "";                        //查询的页码

        public ShowAPIURL(string channelid,string channelname,string needcontent,string needhtml,string title,string page)
        {
            this.channelId = channelid;
            this.channelName = channelname;
            this.needContent = needcontent;
            this.needHtml = needhtml;
            this.title = title;
            this.page = page;
        }       

        public override string ToString()
        {
            this.showapi_timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string resultStr = "https://route.showapi.com/109-35?channelId="
                + channelId + "&channelName=" + channelName + "&needContent=" + needContent
                + "&needHtml=" + needHtml + "&page=" + page + "&showapi_appid=" + showapi_appid
                + "&showapi_timestamp=" + showapi_timestamp + "&title=" + title + "&showapi_sign=" + showapi_sign;
            return resultStr;
        }
    }
}
