using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using Microsoft.Data.Sqlite;
using System.Threading;

namespace Rivers
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int pages = 20;

            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc;
            HtmlNodeCollection htmlNodes;

            for (int page_id = 1; page_id < pages; page_id++)
            {
                doc = web.Load("https://geo.koltyrin.ru/reki.php?page=1");
                htmlNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'field_center')]");

                if (htmlNodes != null)
                    foreach (HtmlNode node in htmlNodes)
                    {
                        HtmlNodeCollection tables = node.SelectNodes("//table");

                        if (tables != null)
                        {
                            HtmlNodeCollection rows = tables[1].SelectNodes("tr");

                            for (int i = 1; i < rows.Count; i++)
                            {
                                HtmlNodeCollection cells = rows[i].SelectNodes("td");

                                string[] data = new string[] {
                                    cells[0].InnerText, cells[2].InnerText, cells[3].InnerText, cells[4].InnerText, cells[5].InnerText
                                };

                                writeToDB(data);
                            }
                        }
                    }
            }
            
            showDB();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clearDB();
            Thread.Sleep(1000);
            showDB();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            showDB();
        }

        public void writeToDB(string[] data)
        {
            using (var connection = new SqliteConnection("Data Source=rivers.db"))
            {
                connection.Open();
                
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "INSERT INTO rivers (name, length, square, tributary, ocean) VALUES ('" + string.Join("', '", data) + "')";

                int number = command.ExecuteNonQuery();
            }
        }

        public void clearDB()
        {
            using (var connection = new SqliteConnection("Data Source=rivers.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "DELETE FROM rivers";

                int number = command.ExecuteNonQuery();
            }
        }

        public void showDB()
        {
            dataGridView1.Rows.Clear();

            using (var connection = new SqliteConnection("Data Source=rivers.db"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM rivers";
                
                using (SqliteDataReader read = command.ExecuteReader())
                {
                    while (read.Read())
                    {
                        dataGridView1.Rows.Add(new object[] {
                            read.GetValue(1),
                            read.GetValue(2),
                            read.GetValue(3),
                            read.GetValue(4),
                            read.GetValue(5),
                        });
                    }
                }

            }
        }
    }
}
