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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class setting : Page
    {
        public setting()
        {
            this.InitializeComponent();
        }
        private Model.user VM; //用来构造json数据
        // get到用户的信息
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
                VM = new Model.user(obj["username"].ToString(), obj["nickname"].ToString(), obj["avatar"].ToString(), "");
                avatar.ImageSource = new BitmapImage(new Uri(obj["avatar"].ToString()));
                name.Text = obj["nickname"].ToString();
            }
            catch(Exception ex)
            {
                await new MessageDialog("怕是你没联网哦").ShowAsync();
            }
        }
        // 类似打卡页面，选择一个图片上传到云床，然后获取url
        private async void SelectPictureButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                avatar.ImageSource = bitmapImage;

                var streamData = await file.OpenStreamForReadAsync();
                var bytes = new byte[(int)streamData.Length];
                streamData.Read(bytes, 0, (int)streamData.Length);

                string imageSrc = await Utils.Qiniu.UploadData(bytes, file.Name.ToString(), VM.username);
                edit_Avatar(imageSrc);
            }
        }
        // 这里是修改服务器数据库的头像url的http请求
        private async void edit_Avatar(string imageSrc)
        {
            try
            {
                HttpClient client = new HttpClient();
                string url = Config.config.prefix + "/editAvatar";
                var user = new Model.user(VM.username, "", imageSrc, "");
                var temp = JsonConvert.SerializeObject(user);
                var content = new StringContent(temp);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var res = await client.PostAsync(url, content);
                string result = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(result);
                if (obj["status"].ToString().Equals("200"))
                {

                }
            }
            catch(Exception ex)
            {
                await new MessageDialog("怕是你没联网哦").ShowAsync();
            }
        }
        // 这里是修改昵称的http请求，昵称不能为空不能超过10个字符
        private async void edit_Click(object sender, RoutedEventArgs e)
        {
            string str = name.Text;
            if (str.Equals(""))
            {
                var i = new MessageDialog("不能为空啊~").ShowAsync();
            }
            else if (str.Length > 10)
            {
                var i = new MessageDialog("昵称别要太长，10个字符就好").ShowAsync();
            }
            else
            {
                try
                {
                    HttpClient client = new HttpClient();
                    string url = Config.config.prefix + "/editNickname";
                    var user = new Model.user(VM.username, str, "", "");
                    var temp = JsonConvert.SerializeObject(user);

                    var content = new StringContent(temp);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var res = await client.PostAsync(url, content);
                    string result = await res.Content.ReadAsStringAsync();
                    var obj = JObject.Parse(result);
                    if (obj["status"].ToString().Equals("200")) {
                        await new MessageDialog("修改成功").ShowAsync();
                    }
                }
                catch(Exception ex)
                {
                    await new MessageDialog("怕是你没联网哦").ShowAsync();
                }

            }
        }
        // 回退按钮的click事件
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
