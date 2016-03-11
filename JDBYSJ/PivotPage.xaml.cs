using JDBYSJ.Common;
using JDBYSJ.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “透视应用程序”模板在 http://go.microsoft.com/fwlink/?LinkID=391641 上有介绍

namespace JDBYSJ
{
    public sealed partial class PivotPage : Page
    {
        private const string SocialNewsName = "Social";
        private const string TechnologyNewsName = "Technology";
        private const string YuleNewsName = "Yule";
        private const string SelfNewsName = "Self";

        private int SocialCurrentPage = 0;
        private int YuleCurrentPage = 0;
        private int TechnologyCurrentPage = 0;
        private int SelfCurrentPage = 0;


       

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
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
        /// 的字典。首次访问页面时，该状态将为 null。</param>
        /// 
        /// 社会新闻项加载数据
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: 创建适用于问题域的合适数据模型以替换示例数据
            if (App.HaveNetWork)
            {
                if (App.IsFirstLoadPivotPage)
                {
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {                        
                        this.Panel_Load.Opacity = 1;
                        this.ProgressRing_Load.IsActive = true;
                    });
                    var socialNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Social);
                    this.defaultViewModel[SocialNewsName] = socialNews;
                    SocialCurrentPage = Convert.ToInt32(socialNews.showapi_res_body.pagebean.currentPage);
                }
            }
        }

        /// <summary>
        /// 滚动到视图中后，为第二个数据透视项加载内容。
        /// 娱乐新闻项加载数据
        /// </summary>
        private async void YulePivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.HaveNetWork)
            {
                if (App.IsFirstLoadPivotPage)
                {
                    var yuleNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Yule);
                    this.defaultViewModel[YuleNewsName] = yuleNews;
                    YuleCurrentPage = Convert.ToInt32(yuleNews.showapi_res_body.pagebean.currentPage);
                }
            }
        }

        //科技新闻项加载数据
        private async void PivotItemTech_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.HaveNetWork)
            {
                if (App.IsFirstLoadPivotPage)
                {
                    var technologyNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Technology);
                    this.defaultViewModel[TechnologyNewsName] = technologyNews;
                    TechnologyCurrentPage = Convert.ToInt32(technologyNews.showapi_res_body.pagebean.currentPage);
                }
            }
        }

        //私人订阅新闻项加载数据
        private async void PivotItemSelf_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.HaveNetWork)
            {
                if (App.SelfChannelID.Length != 0)
                {
                    var selfNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Self);
                    this.defaultViewModel[SelfNewsName] = selfNews;
                    SelfCurrentPage = Convert.ToInt32(selfNews.showapi_res_body.pagebean.currentPage);
                        
                }
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Panel_Load.Opacity = 0;
                    this.ProgressRing_Load.IsActive = false;
                });
                App.IsFirstLoadPivotPage = false;                
            }
            var dispatcher1 = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher1.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.TextBlock_SelfChannelName.Text = App.SelfChannelName;             
            });
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。值必须符合序列化
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/>。</param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: 在此处保存页面的唯一状态。
        }

        ///// <summary>
        ///// 在单击应用程序栏按钮时将项添加到列表中。
        ///// </summary>
        //private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string groupName = this.pivot.SelectedIndex == 0 ? FirstGroupName : SecondGroupName;
        //    var group = this.DefaultViewModel[groupName] as SampleDataGroup;
        //    var nextItemId = group.Items.Count + 1;
        //    var newItem = new SampleDataItem(
        //        string.Format(CultureInfo.InvariantCulture, "Group-{0}-Item-{1}", this.pivot.SelectedIndex + 1, nextItemId),
        //        string.Format(CultureInfo.CurrentCulture, this.resourceLoader.GetString("NewItemTitle"), nextItemId),
        //        string.Empty,
        //        string.Empty,
        //        this.resourceLoader.GetString("NewItemDescription"),
        //        string.Empty);

        //    group.Items.Add(newItem);

        //    // 将新的项滚动到视图中。
        //    var container = this.pivot.ContainerFromIndex(this.pivot.SelectedIndex) as ContentControl;
        //    var listView = container.ContentTemplateRoot as ListView;
        //    listView.ScrollIntoView(newItem, ScrollIntoViewAlignment.Leading);
        //}

        /// <summary>
        /// 在单击节内的项时调用。
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 导航至相应的目标页，并
            // 通过将所需信息作为导航参数传入来配置新页

            // 获取用户单击的项目以及所处的pivot传给ItemPage
            string itemLink = ((NewsItem)e.ClickedItem).link;
            string itemLinkAndType = itemLink + "@" + this.pivot.SelectedIndex.ToString();
            if (!Frame.Navigate(typeof(ItemPage), itemLinkAndType))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
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

        //搜索按钮的单击事件
        private void SearchAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(SearchPage)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private async void RefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch(this.pivot.SelectedIndex)
            {
                case 0:
                    var socialNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Social);
                    this.defaultViewModel[SocialNewsName] = socialNews;
                    SocialCurrentPage = Convert.ToInt32(socialNews.showapi_res_body.pagebean.currentPage);
                    break;
                case 1:
                    var yuleNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Yule);
                    this.defaultViewModel[YuleNewsName] = yuleNews;
                    YuleCurrentPage = Convert.ToInt32(yuleNews.showapi_res_body.pagebean.currentPage);
                    break;
                case 2:
                    var technologyNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Technology);
                    this.defaultViewModel[TechnologyNewsName] = technologyNews;
                    TechnologyCurrentPage = Convert.ToInt32(technologyNews.showapi_res_body.pagebean.currentPage);
                    break;
                case 3:
                    if (App.SelfChannelID.Length != 0)
                    {
                        var selfNews = await NewsDataSource.GetFirstPageShowAPI_NewsClassAsync(NewsChannelsType.Self);
                        this.defaultViewModel[SelfNewsName] = selfNews;
                        SelfCurrentPage = Convert.ToInt32(selfNews.showapi_res_body.pagebean.currentPage);
                    }
                    break;
            }
        }

        private async void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.HaveNetWork)
            {
                switch (this.pivot.SelectedIndex)
                {
                    case 0:
                        ShowAPI_NewsClass socialNews = this.defaultViewModel[SocialNewsName] as ShowAPI_NewsClass;
                        var addSocialNews = await NewsDataSource.GetNextPage(socialNews, NewsChannelsType.Social, SocialCurrentPage);
                        if (addSocialNews.showapi_res_body.pagebean.currentPage != "-1")
                        {
                            SocialCurrentPage = Convert.ToInt32(addSocialNews.showapi_res_body.pagebean.currentPage);
                            for (int i = 0; i < addSocialNews.showapi_res_body.pagebean.contentlist.Count; i++)
                            {
                                socialNews.showapi_res_body.pagebean.contentlist.Add(addSocialNews.showapi_res_body.pagebean.contentlist[i]);
                            }
                        }
                        break;
                    case 1:
                        ShowAPI_NewsClass yuleNews = this.defaultViewModel[YuleNewsName] as ShowAPI_NewsClass;
                        var addYuleNews = await NewsDataSource.GetNextPage(yuleNews, NewsChannelsType.Yule, YuleCurrentPage);
                        if(addYuleNews.showapi_res_body.pagebean.currentPage != "-1")
                        {
                            YuleCurrentPage = Convert.ToInt32(addYuleNews.showapi_res_body.pagebean.currentPage);
                            for (int i = 0; i < addYuleNews.showapi_res_body.pagebean.contentlist.Count; i++)
                            {
                                yuleNews.showapi_res_body.pagebean.contentlist.Add(addYuleNews.showapi_res_body.pagebean.contentlist[i]);
                            }
                        }
                        break;
                    case 2:
                        ShowAPI_NewsClass techNews = this.defaultViewModel[TechnologyNewsName] as ShowAPI_NewsClass;
                        var addTechNews = await NewsDataSource.GetNextPage(techNews, NewsChannelsType.Technology, TechnologyCurrentPage);
                        if(addTechNews.showapi_res_body.pagebean.currentPage != "-1")
                        {
                            TechnologyCurrentPage = Convert.ToInt32(addTechNews.showapi_res_body.pagebean.currentPage);
                            for (int i = 0; i < addTechNews.showapi_res_body.pagebean.contentlist.Count; i++)
                            {
                                techNews.showapi_res_body.pagebean.contentlist.Add(addTechNews.showapi_res_body.pagebean.contentlist[i]);
                            }
                        }
                        break;
                    case 3:
                        if (App.SelfChannelID.Length != 0)
                        {
                            ShowAPI_NewsClass selfNews = this.defaultViewModel[SelfNewsName] as ShowAPI_NewsClass;
                            var addSelfNews = await NewsDataSource.GetNextPage(selfNews, NewsChannelsType.Self, SelfCurrentPage);
                            if (addSelfNews.showapi_res_body.pagebean.currentPage != "-1")
                            {
                                SelfCurrentPage = Convert.ToInt32(addSelfNews.showapi_res_body.pagebean.currentPage);
                                for (int i = 0; i < addSelfNews.showapi_res_body.pagebean.contentlist.Count; i++)
                                {
                                    selfNews.showapi_res_body.pagebean.contentlist.Add(addSelfNews.showapi_res_body.pagebean.contentlist[i]);
                                }
                            }
                        }
                        break;
                }
            }
            else
            {
                MessageDialog errormsgdlg = new MessageDialog("请检查你的网络连接", "警告");
                errormsgdlg.ShowAsync();
            }
        }

        private void SettingAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(SettingPage)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }
    }
}
