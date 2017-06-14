using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using Windows.UI.Xaml.Navigation;
//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace midpro
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ViewModel.dakaVM VM { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            this.VM = new ViewModel.dakaVM(); // 初始化所有的打卡记录
            this.VM.allItemsInit();
        }
        // 汉堡按钮的click事件
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mySplit.IsPaneOpen = !mySplit.IsPaneOpen;
        }
        // 处理导航逻辑，根据内容来确定导航到哪个页面
        private void listView1_ItemClick(object sender, ItemClickEventArgs e)
        {
            TextBlock obj = (TextBlock)e.ClickedItem;
            string expression = obj.Text.ToString();

            switch (expression)
            {
                case "主页": break;
                case "注销": logoutHandle(); break;
                case "打卡": Frame.Navigate(typeof(daKa)); break;
                case "修改信息": Frame.Navigate(typeof(setting));break;
                case "我的打卡": Frame.Navigate(typeof(myDaKas)); break;
            }
        }
        // 点击某一个打卡记录，去到该打卡的详情页面
        private void daka_ItemClick(object sender, ItemClickEventArgs e)
        {
            VM.SelectedItem = (Model.daKa)(e.ClickedItem);
            if (VM.SelectedItem != null)
            {
                Frame.Navigate(typeof(details), VM);
            }

        }
        // 刷新按钮的click事件
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            VM.allItemsInit();
        }
        // 登出的操作，把用户信息从数据库中删除，并且发个http请求，清除cookies
        private async void logoutHandle()
        {
            // 本地数据库操作
            using (var statement = App.conn.Prepare("DELETE FROM user WHERE anchor = ?"))
            {
                statement.Bind(1, "zhulifeng");
                statement.Step();
            }
            // 登出http请求
            HttpClient client = new HttpClient();
            try
            {
                var _result = await client.GetAsync(Config.config.prefix + "/logout");
                string _content = await _result.Content.ReadAsStringAsync();

                var obj = JObject.Parse(_content);
                if (obj["status"].ToString().Equals("200"))
                {
                    var i = new MessageDialog(obj["message"].ToString()).ShowAsync();
                    // 退出成功，回到登录页面
                    Frame.Navigate(typeof(login));
                }
            }
            catch(Exception ex)
            {
                var i = new MessageDialog("怕是你没有连网哦").ShowAsync();
            }
        }
    }
}
