using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace midpro.Model
{
    class comment
    {
        public string nickname;
        public string id;
        public string content;

        public comment(string nickname, string id, string content)
        {
            this.nickname = nickname;
            this.id = id;
            this.content = content;
        }
    }
}
