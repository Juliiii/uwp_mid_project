using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace midpro.ViewModel
{
    class dakaVM
    {
        private ObservableCollection<Model.daKa> allItems = new ObservableCollection<Model.daKa>();
        public ObservableCollection<Model.daKa> AllItems { get { return this.allItems; } }

        private ObservableCollection<Model.daKa> myItems = new ObservableCollection<Model.daKa>();
        public ObservableCollection<Model.daKa> MyItems { get { return this.myItems; } }

        private Model.daKa selectedItem = default(Model.daKa);
        public Model.daKa SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }
        public dakaVM() { }
        // 初始化当前用户的打卡记录
        public async void myItemsInit()
        {
            try
            {
                this.myItems.Clear();
                HttpClient req = new HttpClient();
                string url = Config.config.prefix + "/records";
                var res = await req.GetAsync(url);
                string content = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);
                int length = Int32.Parse(obj["length"].ToString());

                for (int i = 0; i < length; i++)
                {
                    var cur = JObject.Parse(obj["data"][i].ToString());//obj["data"][i];
                    var src = cur["srcUri"].ToString();
                    var img = src.Equals("") ? null : new BitmapImage(new Uri(src));

                    this.myItems.Add(new Model.daKa(cur["_id"].ToString(),
                                                    cur["nickname"].ToString(),
                                                    new BitmapImage(new Uri(cur["avatarUri"].ToString())),
                                                    cur["avatarUri"].ToString(),
                                                    cur["username"].ToString(),
                                                    cur["content"].ToString(),
                                                    cur["createdTime"].ToString(),
                                                    img,
                                                    src));
                }
            } catch (Exception ex)
            {
                await new MessageDialog("怕是你在搞事噢").ShowAsync();
            }
        }
        // 初始化所有的记录
        public async void allItemsInit()
        {
            try
            {
                this.allItems.Clear();
                HttpClient req = new HttpClient();
                string url = Config.config.prefix + "/dakas";
                var res = await req.GetAsync(url);
                string content = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);
                int length = Int32.Parse(obj["length"].ToString());

                for (int i = 0; i < length; i++)
                {
                    var cur = JObject.Parse(obj["data"][i].ToString());
                    var src = cur["srcUri"].ToString();
                    var img = src.Equals("") ? null : new BitmapImage(new Uri(src));

                    this.allItems.Add(new Model.daKa(cur["_id"].ToString(),
                                                    cur["nickname"].ToString(),
                                                    new BitmapImage(new Uri(cur["avatarUri"].ToString())),
                                                    cur["avatarUri"].ToString(),
                                                    cur["username"].ToString(),
                                                    cur["content"].ToString(),
                                                    cur["createdTime"].ToString(),
                                                    img,
                                                    src));
                }
            }
            catch(Exception ex)
            {
                await new MessageDialog("怕是你在搞事噢").ShowAsync();
            }
        }
    }
}
