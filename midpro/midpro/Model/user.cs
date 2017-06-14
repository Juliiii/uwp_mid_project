using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace midpro.Model
{
    class user
    {
        public string username { get; set; }
        public string nickname { get; set; }
        public string password { get; set; }
        public string avatar { get; set; }

        public user(string username, string nickname, string avatar, string password)
        {
            this.username = username;
            this.nickname = nickname;
            this.avatar = avatar;
            this.password = password;
        }
    }
}
