using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace midpro.ViewModel
{
    class commentVM
    {
        private ObservableCollection<Model.comment> allItems = new ObservableCollection<Model.comment>();
        public ObservableCollection<Model.comment> AllItems { get { return this.allItems; } }

        public commentVM() {}
        // 初始化某条打卡动态的所有评论
        public void AddItem(JObject obj)
        {
            this.allItems.Clear();
            int length = Int32.Parse(obj["length"].ToString());

            for (int i = 0; i < length; i++)
            {
                var cur = JObject.Parse(obj["data"][i].ToString());

                this.allItems.Add(new Model.comment(cur["nickname"].ToString(),
                                                cur["_id"].ToString(),
                                                cur["content"].ToString()));
            }
        }
    }
}
