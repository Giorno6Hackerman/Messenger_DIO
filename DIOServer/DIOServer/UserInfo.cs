using System;
using System.Collections.Generic;

namespace DIOServer
{
    [Serializable]
    public class UserInfo
    {
        public string UserName;
        public string Password;
        [NonSerialized]
        public bool LoggedIn;      // Is logged in and connected?
        [NonSerialized]
        public DIOClient Connection;  // Connection info
        public string imagePath;
        public List<string> friends;

        public UserInfo(string user, string pass)
        {
            this.UserName = user;
            this.Password = pass;
            this.LoggedIn = false;
            friends = new List<string>();
        }
        public UserInfo(string user, string pass, DIOClient conn)
        {
            this.UserName = user;
            this.Password = pass;
            this.LoggedIn = true;
            this.Connection = conn;
        }
    }
}
