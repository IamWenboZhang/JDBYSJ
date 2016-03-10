using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “用户控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=234236 上有介绍

namespace JDBYSJ
{
    public sealed partial class RefreshListView : UserControl
    {
        //DispatcherTimer toptimer = new DispatcherTimer();
        //DispatcherTimer timer = new DispatcherTimer();
        private ScrollViewer listViewScrollViewer;

        private double listviewItemHeight = -1;
        private int optLimitHeight = 96;
        private bool isInPrpcessing = false;

        private Point eventStartPoint;

        private double lastDeltaHandledY = 0;

        //private ObservableCollection<dynamic> listDataItems = new ObservableCollection<dynamic>();

        ////移动开始位置
        //private Point moveStartPoint;
        ////是否可以移动
        //private bool canMove = false;

        public RefreshListView()
        {
            this.InitializeComponent();

            //this.imgArrow.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/Arrow_big_down_99.082051282051px_1197958_easyicon.net.png", UriKind.RelativeOrAbsolute));
            this.LayoutUpdated += ForumList_LayoutUpdated;
            this.recRefresh.SizeChanged += recRefresh_SizeChanged;
            this.ForumList.ManipulationStarted += ForumList_ManipulationStarted;
            this.ForumList.ManipulationDelta += ForumList_ManipulationDelta;
            this.ForumList.ManipulationCompleted += ForumList_ManipulationCompleted;
            this.ForumList.SelectionChanged+=ForumList_SelectionChanged;
            this.ForumList.ItemClick += ForumList_ItemClick;
        }

        //可视化树的布局更改事件
        private void ForumList_LayoutUpdated(object sender, object e)
        {
            if (this.Height.Equals(double.NaN) && this.Parent != null)
            {
                this.Height = ((Windows.UI.Xaml.FrameworkElement)(this.Parent)).ActualHeight;
            }
            if (listViewScrollViewer == null)
            {
                listViewScrollViewer = FindVisualElement<ScrollViewer>(VisualTreeHelper.GetParent(this));
                listViewScrollViewer.ManipulationMode = ManipulationModes.All;
            }

            for (int i = 0; i < this.ForumList.Items.Count; i++)
            {
                ListViewItem item = this.ForumList.ContainerFromIndex(i) as ListViewItem;

                if (item != null && item.ManipulationMode != ManipulationModes.All)
                {
                    item.ManipulationMode = ManipulationModes.All;

                    if (listviewItemHeight < 0)
                    {
                        listviewItemHeight = ((Windows.UI.Xaml.FrameworkElement)(item)).ActualHeight;
                    }
                }
            }
        }

        private static T FindVisualElement<T>(DependencyObject container) where T : DependencyObject
        {
            Queue<DependencyObject> childQueue = new Queue<DependencyObject>();
            childQueue.Enqueue(container);

            while (childQueue.Count > 0)
            {
                DependencyObject current = childQueue.Dequeue();

                T result = current as T;
                if (result != null && result != container)
                {
                    return result;
                }

                int childCount = VisualTreeHelper.GetChildrenCount(current);
                for (int childIndex = 0; childIndex < childCount; childIndex++)
                {
                    childQueue.Enqueue(VisualTreeHelper.GetChild(current, childIndex));
                }
            }

            return null;
        }


        private void recRefresh_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > this.optLimitHeight && !isInPrpcessing)
            {
                if (!this.txtOperationTip.Text.Equals("松开刷新…"))
                {
                    this.txtOperationTip.Text = "松开刷新…";
                    //this.ImgStoryBoard.Begin();                    
                }
            }
        }

        //在输入设备对控件进行操作开始前的事件
        private void ForumList_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            eventStartPoint = e.Position;
            lastDeltaHandledY = 0;
            if (!isInPrpcessing)
            {
                this.txtOperationTip.Text = "下拉刷新列表…";
            }
            e.Handled = true;
        }

        //输入设备对控件操作期间位置发生了改变
        private void ForumList_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double offset = e.Cumulative.Translation.Y - lastDeltaHandledY;
            if (listViewScrollViewer.VerticalOffset < 3)
            {
                double height = recRefresh.Height + offset;
                recRefresh.Height = height > 0 ? height : 0;
                if (offset < 0 && recRefresh.Height <= 0)
                {
                    //向上滑动且刷新Panel未显示时滚动条下移
                    listViewScrollViewer.ScrollToVerticalOffset(listViewScrollViewer.VerticalOffset - offset / listviewItemHeight);
                    //listViewScrollViewer.ChangeView(0, listViewScrollViewer.VerticalOffset - offset / listviewItemHeight, 0);
                }
            }
            else if (listViewScrollViewer.VerticalOffset >= listViewScrollViewer.ScrollableHeight - 1)
            {
                double height = recLoad.Height - offset;
                recLoad.Height = height > 0 ? height : 0;
                if (offset > 0 && recLoad.Height <= 0)
                {
                    listViewScrollViewer.ScrollToVerticalOffset(listViewScrollViewer.VerticalOffset - offset / listviewItemHeight);
                    //listViewScrollViewer.ChangeView(0, listViewScrollViewer.VerticalOffset - offset / listviewItemHeight, 0);
                }
            }
            else
            {
                listViewScrollViewer.ScrollToVerticalOffset(listViewScrollViewer.VerticalOffset - offset / listviewItemHeight);
                //listViewScrollViewer.ChangeView(0, listViewScrollViewer.VerticalOffset - offset / listviewItemHeight, 0);
            }
            lastDeltaHandledY = e.Cumulative.Translation.Y;
            e.Handled = true;
        }


        //设备对控件操作完成后触发的事件
        private void ForumList_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (listViewScrollViewer != null)
            {
                double offset = listViewScrollViewer.VerticalOffset;
                double total = listViewScrollViewer.ScrollableHeight;
                if (offset <= 3 || offset > total - 2)
                {
                    DoListItemsSwipe();
                }
            }
        }
        #region 向本控件注入ListView的属性

        public static readonly DependencyProperty IsItemClickEnabledProperty
            = DependencyProperty.Register("IsItemClickEnabled", typeof(bool), typeof(RefreshListView), new PropertyMetadata(false));

        public bool IsItemClickEnabled
        {
            get { return (bool)base.GetValue(IsItemClickEnabledProperty); }
            set
            {
                base.SetValue(IsItemClickEnabledProperty, value);
                this.ForumList.IsItemClickEnabled = value;
            }
        }


        public static readonly DependencyProperty ItemsSourceProperty
            = DependencyProperty.Register("ItemsSource", typeof(object), typeof(RefreshListView), new PropertyMetadata(null));

        public object ItemsSource
        {
            get { return base.GetValue(ItemsSourceProperty); }
            set
            {
                base.SetValue(ItemsSourceProperty, value);
                this.ForumList.ItemsSource = value;
;            }
        }       

        /// <summary>
        /// 数据加载后是否可下拉刷新
        /// </summary>

        public static readonly DependencyProperty RefreshableProperty
            = DependencyProperty.Register("Refreshable", typeof(bool), typeof(RefreshListView), new PropertyMetadata(true));

        public bool Refreshable
        {
            get
            {
                return (bool)base.GetValue(RefreshableProperty);
            }
            set
            {
                base.SetValue(RefreshableProperty, value);
                if (!value)
                {
                    //this.imgArrow.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    //this.prgRefresh.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    this.txtOperationTip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    this.txtOperationTime.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        //数据模板
        public static readonly DependencyProperty ItemTemplatePerporty
            = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(RefreshListView), new PropertyMetadata(null));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)base.GetValue(ItemTemplatePerporty); }
            set
            {
                base.SetValue(ItemTemplatePerporty, value);
                this.ForumList.ItemTemplate = value;
                
            }
        }

       
        #endregion

        //划动ListItem事件
        private void DoListItemsSwipe()
        {
            if (recRefresh.Height > 0)
            {
                if (recRefresh.Height > optLimitHeight && this.Refreshable)
                {
                    recRefresh.Height = 96;
                    //imgArrow.Visibility = Visibility.Collapsed;
                    //prgRefresh.Visibility = Visibility.Visible;
                    if (!this.isInPrpcessing)
                    {
                        System.Diagnostics.Debug.WriteLine(this.isInPrpcessing);
                        DoListDataSourceRefresh();
                    }
                }
                else
                {
                    recRefresh.Height = 0;
                }
                listViewScrollViewer.ScrollToVerticalOffset(2.05);
                //listViewScrollViewer.ChangeView(0, 2.05, 0);
            }
            if (recLoad.Height > 0)
            {
                if (recLoad.Height > optLimitHeight)
                {
                    recLoad.Height = 68;
                    splNextPageLoading.Visibility = Visibility.Visible;
                    if (!this.isInPrpcessing)
                    {
                        DoListDataLoadNextPage();
                    }
                }
                else
                {
                    recLoad.Height = 0;
                }
            }
        }


        private void DoListDataSourceRefresh()
        {
           
            string timeNowStr = DateTime.Now.ToString("MM-dd HH:mm");
            this.isInPrpcessing = true;

            this.txtOperationTip.Text = "加载数据中";

            this.txtOperationTime.Text = "上次刷新时间:" + timeNowStr;

            this.isInPrpcessing = false;

            this.recRefresh.Height = 0;

            //this.imgArrow.Visibility = Windows.UI.Xaml.Visibility.Visible;

            //this.prgRefresh.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            //this.ImgStoryBoard.Begin(); //箭头复位
            
            MessageDialog msgdlg = new MessageDialog("刷新数据中");
            msgdlg.ShowAsync();

        }

        private void DoListDataLoadNextPage()
        {
            this.recLoad.Height = 0;
            splNextPageLoading.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public event SelectionChangedEventHandler SelectionChanged;

        private void ForumList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged(sender, e);           
        }

        public event ItemClickEventHandler ItemClick;

        private void ForumList_ItemClick(object sender,ItemClickEventArgs e)
        {
            ItemClick(sender,e);        
        }
    }
}
