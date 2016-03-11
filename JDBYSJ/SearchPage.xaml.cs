using JDBYSJ.Common;
using JDBYSJ.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace JDBYSJ
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {       
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        private int SearchCurrentPage = 0;
        public static string MainWord = "";

        public SearchPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 获取此 <see cref="Page"/> 的视图模型。
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        ///使用在导航过程中传递的内容填充页。 在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源；通常为 <see cref="NavigationHelper"/>。
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 字典。首次访问页面时，该状态将为 null。</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: 创建适用于问题域的合适数据模型以替换示例数据。
            var searchNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Search);
            this.defaultViewModel["SearchResult"] = searchNews;
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。值必须符合
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/>。</param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: 在此处保存页面的唯一状态。
        }

        #region NavigationHelper 注册

        /// <summary>
        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// <para>
        /// 应将页面特有的逻辑放入用于
        /// <see cref="NavigationHelper.LoadState"/>
        /// 和 <see cref="NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。
        /// </para>
        /// </summary>
        /// <param name="e">提供导航方法数据和
        /// 无法取消导航请求的事件处理程序。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 获取用户单击的项目以及所处的pivot传给ItemPage
            string itemLink = ((NewsItem)e.ClickedItem).link;
            string itemLinkAndType = itemLink + "@-1" ;
            if (!Frame.Navigate(typeof(ItemPage), itemLinkAndType))
            {
                throw new Exception();
            }
        }

        private async void Btn_Search_Click(object sender, RoutedEventArgs e)
        {
            if (App.HaveNetWork)
            {
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                try
                {
                    await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.StackPanel_Result.Opacity = 0;
                        this.Panel_Search.Opacity = 1;
                        this.ProgressRing_Search.IsActive = true;
                    });
                    MainWord = this.TextBox_Search.Text.Trim();
                    var searchNews = await NewsDataSource.SearchNews(MainWord);
                    SearchCurrentPage = Convert.ToInt32(searchNews.showapi_res_body.pagebean.currentPage);
                    if (searchNews.showapi_res_body.pagebean.allNum == "0")
                    {
                        MessageDialog none_msgdlg = new MessageDialog("未搜索到相关消息", "换个词试试吧~");
                        none_msgdlg.ShowAsync();
                    }
                    else
                    {
                        await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            this.TextBlock_ResultNumber.Text = searchNews.showapi_res_body.pagebean.allNum;
                            this.StackPanel_Result.Opacity = 1;                                                     
                        });
                    }
                    this.defaultViewModel["SearchResult"] = searchNews;
                }
                catch (Exception ex)
                {                    
                }
                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {                    
                    this.Panel_Search.Opacity = 0;
                    this.ProgressRing_Search.IsActive = false;
                });
            }
            else
            {
                MessageDialog errormsgdlg = new MessageDialog("请检查你的网络连接", "警告");
                errormsgdlg.ShowAsync();
            }
        }

        private async void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            var searchNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Search);
            this.defaultViewModel["SearchResult"] = searchNews;
            SearchCurrentPage = Convert.ToInt32(searchNews.showapi_res_body.pagebean.currentPage);
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.TextBlock_ResultNumber.Text = searchNews.showapi_res_body.pagebean.allNum;
                this.StackPanel_Result.Opacity = 1;
            });
        }

        private async void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            ShowAPI_NewsClass searchNews = this.defaultViewModel["SearchResult"] as ShowAPI_NewsClass;
            var addSearchNews = await NewsDataSource.GetNextPage(searchNews, NewsChannelsType.Search, SearchCurrentPage);
            if (addSearchNews.showapi_res_body.pagebean.currentPage != "-1")
            {
                SearchCurrentPage = Convert.ToInt32(addSearchNews.showapi_res_body.pagebean.currentPage);
                for (int i = 0; i < addSearchNews.showapi_res_body.pagebean.contentlist.Count; i++)
                {
                    searchNews.showapi_res_body.pagebean.contentlist.Add(addSearchNews.showapi_res_body.pagebean.contentlist[i]);
                }
            }
        }
    }
}
