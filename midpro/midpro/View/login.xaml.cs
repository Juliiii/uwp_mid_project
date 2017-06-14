using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Qiniu.Http;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace midpro
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class login : Page
    {
        public login()
        {
            this.InitializeComponent();
        }

        // 登录按钮点击事件
        private void userLogin_Click(object sender, RoutedEventArgs e)
        {
            this.login_Handle();
        }
        // 登录成功的处理事件，将登录者信息存在用户的本地数据库
        private async void success_Handle()
        {
            var client = new HttpClient();
            var _result = await client.GetAsync(Config.config.prefix + "/user");
            string _content = await _result.Content.ReadAsStringAsync();

            var obj = JObject.Parse(_content);



            using (var custstmt = App.conn.Prepare("INSERT INTO user (username, nickname, avatar, anchor) VALUES (?, ?, ?, ?)"))
            {
                custstmt.Bind(1, obj["username"].ToString());
                custstmt.Bind(2, obj["nickname"].ToString());
                custstmt.Bind(3, obj["avatar"].ToString());
                custstmt.Bind(4, "zhulifeng");
                custstmt.Step();
            }

            Model.user _user = new Model.user(obj["username"].ToString(), obj["nickname"].ToString(), obj["avatar"].ToString(), "");
            Frame.Navigate(typeof(MainPage));
        }
        // 登录按钮的keydown事件
        private void userLogin_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var key = e.Key.ToString();
            if (key.Equals("Enter") || key.Equals("Space"))
            {
                this.login_Handle();
            }
        }
        // 登录的http请求
        private async void login_Handle()
        {
            string name = username.Text;
            string psd = password.Password;

            if (name.Equals("") || psd.Equals(""))
            {
                var i = new MessageDialog("不能留空噢~").ShowAsync();
                return;
            }
            HttpClient client = new HttpClient();
            string url = Config.config.prefix + "/session";
            var user = new Model.user(name, "", null, psd);
            var temp = JsonConvert.SerializeObject(user);

            var content = new StringContent(temp);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage res;
            try
            {
                res = await client.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                var i = new MessageDialog("怕是你没连网哦").ShowAsync();
                return;
            }
            string result = await res.Content.ReadAsStringAsync();
            var resJson = JObject.Parse(result);
            if (resJson["status"].ToString().Equals("200"))
            {
                await new MessageDialog(resJson["message"].ToString()).ShowAsync();

                success_Handle();
            }
            else
            {
                await new MessageDialog(resJson["message"].ToString()).ShowAsync();
            }
        }
        // 跳转到注册页面的keydown事件
        private void userSignup_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var key = e.Key.ToString();
            if (key.Equals("Enter") || key.Equals("Space"))
            {
                Frame.Navigate(typeof(signup));
            }
        }
        // 跳转到注册页面的click事件
        private void userSignup_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(signup));
        }
    }
}
