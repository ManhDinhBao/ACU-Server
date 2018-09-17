using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class GroupUser
    {
        public GroupUser() { }

        public GroupUser(string id, string name, List<User> users)
        {
            ID = id;
            Name = name;
            Users = users;
        }

        public GroupUser(string id, string name)
        {
            ID = id;
            Name = name;
        }

        private string Id;

        public string ID
        {
            get { return Id; }
            set { Id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private List<User> users;

        public List<User> Users
        {
            get { return users; }
            set { users = value; }
        }

        public List<GroupUser> GetUserGroup()
        {
            List<GroupUser> groupUsers = new List<GroupUser>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.GroupUserQuery("Q", Id, name);
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string groupId = dr["groupId"].ToString();
                    string groupName = dr["groupName"].ToString();
                    users = User.GetUsersByGroup(groupId);

                    GroupUser groupUser = new GroupUser(groupId, groupName, users);
                    groupUsers.Add(groupUser);
                }
                return groupUsers;
            }
            catch (Exception ex)
            {
                return groupUsers;
            }
        }
    }
}
