using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class Card
    {
        public Card() { }
        public Card(string Id,string cardNo)
        {
            Id = id;
            CardNo = cardNo;
        }

        private string id;

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        private string cardNo;

        public string CardNo
        {
            get { return cardNo; }
            set { cardNo = value; }
        }

        public static List<Card> LoadCardByUserId(string userId)
        {
            List<Card> cards = new List<Card>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.CardDetailQuery("Q", userId, "");
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string Id = dr["cardId"].ToString();
                    string CardNo = dr["cardNo"].ToString();

                    Card card = new Card(Id, CardNo);
                    cards.Add(card);
                }
                return cards;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
