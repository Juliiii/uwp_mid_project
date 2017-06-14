using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Qiniu.Http;
using Qiniu.IO;
using Qiniu.IO.Model;
using Qiniu.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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
    public sealed partial class daKa : Page
    {
        private string imageSrc = ""; // 纪录上传图片获得url
        private Model.user _user; // 用来作为构造json数据的类
        public daKa()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }
        // 导航到该页面时，获取当前用户的信息
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.getUser();
        }
        // 一个http请求，获取当前用户的信息
        private async void getUser()
        {
            try
            {
                HttpClient client = new HttpClient();
                var _result = await client.GetAsync(Config.config.prefix + "/user");
                string _content = await _result.Content.ReadAsStringAsync();

                var obj = JObject.Parse(_content);
                _user = new Model.user(obj["username"].ToString(),
                                       obj["nickname"].ToString(),
                                       obj["avatar"].ToString(),
                                       "");
            } catch(Exception ex)
            {
                await new MessageDialog("怕是你没有联网哦").ShowAsync();
            }
        }
        // 构造live tiles，当发布一个动态时调用
        private void live_Tiles(string content, string createdTime)
        {
            XmlDocument tile = new XmlDocument();
            tile.LoadXml(File.ReadAllText("tile.xml"));
            XmlNodeList tileText = tile.GetElementsByTagName("text");
            for (int i = 0; i < tileText.Count; i++)
            {
                ((XmlElement)tileText[i]).InnerText = content;
                i++;
                ((XmlElement)tileText[i]).InnerText = createdTime.ToString();

            }


            XmlNodeList tileImg = tile.GetElementsByTagName("image");
            // 如果该动态是没有图片的，那么就用一个默认的图片，否则就用用户自己上传的图片
            string src = this.imageSrc.Equals("") ? "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcSgyZzd1e7Zdf_JEuhUlwXIZO6DI3Rpc5MiSK2GealBo9yKk5tVJaYHjepP8Q"
                                                  : this.imageSrc.ToString();
            for (int i = 0; i < tileImg.Count; i++)
            {
                ((XmlElement)tileImg[i]).SetAttribute("src", src);
            }
            TileNotification notification = new TileNotification(tile);
            var updator = TileUpdateManager.CreateTileUpdaterForApplication();
            updator.Update(notification);
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string content = textBox.Text;
            if (content.Length > 140)
            {
                var i =  new MessageDialog("长度太长啦~").ShowAsync();
                return;
            }

            // 以下是对时间格式化的一段代码
            DateTime now = DateTime.Now;
            StringBuilder createdTime = new StringBuilder();
            createdTime.Append(now.Year.ToString());
            createdTime.Append("-");
            createdTime.Append(now.Month < 10 ? "0" + now.Month.ToString() : now.Month.ToString());
            createdTime.Append("-");
            createdTime.Append(now.Day < 10 ? "0" + now.Day.ToString() : now.Day.ToString());
            createdTime.Append(" ");
            createdTime.Append(now.Hour < 10 ? "0" + now.Hour.ToString() : now.Hour.ToString());
            createdTime.Append(":");
            createdTime.Append(now.Minute < 10 ? "0" + now.Minute.ToString() : now.Minute.ToString());

            // 调用live tiles 构造函数
            live_Tiles(content, createdTime.ToString());
            // 把这个动态送到后端
            postFrom(content, this.imageSrc, createdTime.ToString());
        }
        // 从本地选择图片，上传图片到云床
        private async void SelectPictureButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            // 限制选择的文件
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                img.Visibility = Visibility.Visible;
                // 将storagefile 转成 二进制数组
                var streamData = await file.OpenStreamForReadAsync();
                var bytes = new byte[(int)streamData.Length];
                streamData.Read(bytes, 0, (int)streamData.Length);
                // 这里是上传图片，得到发回来的url
                this.imageSrc = await Utils.Qiniu.UploadData(bytes, file.Name.ToString(), _user.username);
                // 这里是将图片显示在屏幕上
                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                img.Source = bitmapImage;
            }
        }
        // 一个http请求，送到后端
        public async void postFrom(string content, string imageUrl, string createdTime)
        {
            try
            {
                HttpClient client = new HttpClient();
                // url
                string url = Config.config.prefix + "/records";
                // data 数据
                var user = new Model.daKa("", _user.nickname, null, _user.avatar, _user.username, content, createdTime, null, imageUrl);
                var temp = JsonConvert.SerializeObject(user);
                var _content = new StringContent(temp);
                _content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                // post 请求
                var res = await client.PostAsync(url, _content);
                string result = await res.Content.ReadAsStringAsync();
                // 将结果转成json对象
                var resJson = JObject.Parse(result);
                // 状态码为200操作
                if (resJson["status"].ToString().Equals("200"))
                {
                    await new MessageDialog(resJson["message"].ToString()).ShowAsync();
                    Frame.Navigate(typeof(MainPage), _user);
                }
                else
                {
                    // 以下是错误的操作
                    await new MessageDialog(resJson["message"].ToString()).ShowAsync();
                }
            }
            catch(Exception ex)
            {
                await new MessageDialog("怕是你没有联网哦").ShowAsync();
            }
        }
        // 点击回退按钮，回退到主页
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
