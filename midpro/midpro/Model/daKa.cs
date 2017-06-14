using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace midpro.Model
{
    class daKa
    {
        public string id { get; set; }
        public string username { get; set; }
        public string nickname { get; set; }
        public string content { get; set; }
        public string createdTime { get; set; }
        public string avatarUri { get; set; }
        public string srcUri { get; set; }
        public ImageSource src { get; set; }
        public ImageSource avatar { get; set; }


        public daKa(string id,string nickname, ImageSource avatar, string avatarUri,string username, string content, string createdTime, ImageSource src, string uri)
        {
            this.id = id;
            this.nickname = nickname;
            this.avatar = avatar;
            this.avatarUri = avatarUri;
            this.username = username;
            this.content = content;
            this.createdTime = createdTime;
            this.src = src;
            this.srcUri = uri;
        }
    }
}
