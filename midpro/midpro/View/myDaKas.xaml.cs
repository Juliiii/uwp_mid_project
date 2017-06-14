using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace midpro
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class myDaKas : Page
    {
        ViewModel.dakaVM VM { get; set; } // 用来获得我的打卡的列表
        public myDaKas()
        {
            this.InitializeComponent();
            this.VM = new ViewModel.dakaVM();
            this.VM.myItemsInit(); // 初始化我的打卡列表
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
            this.getUser();
        }
        private async void getUser()
        {
            try
            {
                HttpClient client = new HttpClient();
                var _result = await client.GetAsync(Config.config.prefix + "/user");
                string _content = await _result.Content.ReadAsStringAsync();

                var obj = JObject.Parse(_content);
                avatar.ImageSource = new BitmapImage(new Uri(obj["avatar"].ToString()));
                nickname.Text = obj["nickname"].ToString();
            } catch (Exception ex)
            {
                await new MessageDialog("怕是你没有联网哦").ShowAsync();
            }
        }
        // share打卡内容给朋友
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
        }
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            dynamic ori = e.OriginalSource;
            VM.SelectedItem = (Model.daKa)ori.DataContext;
            DataTransferManager.ShowShareUI();
        }
        private void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            DataRequestDeferral getFile = args.Request.GetDeferral();
            if (VM.SelectedItem.Equals(null)) return;
            request.Data.Properties.Title = VM.SelectedItem.nickname.ToString();
            request.Data.SetText(VM.SelectedItem.content.ToString());
            if (VM.SelectedItem.srcUri != "")
            {
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(VM.SelectedItem.srcUri.ToString())));
            }
            getFile.Complete();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            VM.SelectedItem = (Model.daKa)(e.ClickedItem);
            if (VM.SelectedItem != null)
            {
                Frame.Navigate(typeof(details), VM);
            }
        }
        // 回退按钮的click事件
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
