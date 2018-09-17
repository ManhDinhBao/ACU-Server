using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class User
    {
        public User() { }

        public User(string id, string name, List<Card> cards)
        {
            ID = id;
            Name = name;
            Cards = cards;
        }

        public User(string id, string name)
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

        private List<Card> cards;

        public List<Card> Cards
        {
            get { return cards; }
            set { cards = value; }
        }

        public List<User> GetUsers()
        {
            List<User> users = new List<User>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.UserQuery("Q", Id, name, "");
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string Id = dr["userId"].ToString();
                    string Name = dr["personName"].ToString();
                    List<Card> cards = Card.LoadCardByUserId(Id);

                    User user = new User(Id, Name, cards);
                    users.Add(user);
                }
                return users;
            }
            catch (Exception ex)
            {
                return users;                
            }
        }

        public static List<User> GetUsersByGroup(string groupId)
        {
            List<User> users = new List<User>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.GroupUserDetailQuery("Q", groupId, "");
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string Id = dr["userId"].ToString();
                    string Name = dr["personName"].ToString();
                    List<Card> cards = Card.LoadCardByUserId(Id);

                    User user = new User(Id, Name, cards);
                    users.Add(user);
                }
                return users;
            }
            catch (Exception ex)
            {
                return users;
            }
        }

    }
}
