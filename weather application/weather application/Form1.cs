using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Drawing.Drawing2D;

namespace weather_application
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        }

        string city;
        bool errorShown = false;

        private XDocument GetWeatherData(string city, int days)
        {
            string uri = string.Format("http://api.weatherapi.com/v1/forecast.json?key=93aecf58f8e54b409b561213240311&q={0}&days={1}&aqi=no&alerts=no", city, days);

            using (WebClient client = new WebClient())
            {
                try
                {
                    string jsonString = client.DownloadString(uri);
                    JObject json = JObject.Parse(jsonString);
                    XDocument xml = JsonConvert.DeserializeXNode(json.ToString(), "Root");
                    return xml;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error fetching weather data: ");
                    return null;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            city = txtcity.Text;
            XDocument doc = GetWeatherData(city, 1);

            if (doc != null)
            {
                string iconUri = (string)doc.Descendants("icon").FirstOrDefault();
                if (!string.IsNullOrEmpty(iconUri))
                {
                    try
                    {
                        WebClient client = new WebClient();
                        byte[] image = client.DownloadData("http:" + iconUri);
                        using (MemoryStream stream = new MemoryStream(image))
                        {
                            Bitmap newBitmap = new Bitmap(stream);
                            pictureBox1.Image = newBitmap;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading icon image: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Icon URI is invalid or not available.");
                }

                // Other weather details
                txtmaxtemp.Text = (string)doc.Descendants("maxtemp_c").FirstOrDefault();
                txtmintemp.Text = (string)doc.Descendants("mintemp_c").FirstOrDefault();
                txtwindm.Text = (string)doc.Descendants("maxwind_mph").FirstOrDefault();
                txtwindk.Text = (string)doc.Descendants("maxwind_kph").FirstOrDefault();
                txthumidiity.Text = (string)doc.Descendants("avghumidity").FirstOrDefault();
                label7.Text = (string)doc.Descendants("text").FirstOrDefault();
                txtcountry.Text = (string)doc.Descendants("country").FirstOrDefault();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Country", typeof(string));
            dt.Columns.Add("Date", typeof(string));
            dt.Columns.Add("Max Temp (°C)", typeof(string));
            dt.Columns.Add("Min Temp (°C)", typeof(string));
            dt.Columns.Add("Max Wind (mph)", typeof(string));
            dt.Columns.Add("Max Wind (kph)", typeof(string));
            dt.Columns.Add("Humidity (%)", typeof(string));
            dt.Columns.Add("Cloud", typeof(string));
            dt.Columns.Add("Icon", typeof(Bitmap));

            city = txtcity.Text;
            XDocument doc = GetWeatherData(city, 7);

            if (doc != null)
            {
                foreach (var day in doc.Descendants("forecastday"))
                {
                    string date = (string)day.Element("date");
                    string maxtemp = (string)day.Descendants("maxtemp_c").FirstOrDefault();
                    string mintemp = (string)day.Descendants("mintemp_c").FirstOrDefault();
                    string maxwindm = (string)day.Descendants("maxwind_mph").FirstOrDefault();
                    string maxwindk = (string)day.Descendants("maxwind_kph").FirstOrDefault();
                    string humidity = (string)day.Descendants("avghumidity").FirstOrDefault();
                    string country = (string)doc.Descendants("country").FirstOrDefault();
                    string cloud = (string)day.Descendants("text").FirstOrDefault();

                    string iconUri = (string)day.Descendants("icon").FirstOrDefault();
                    Bitmap iconBitmap = null;

                    if (!string.IsNullOrEmpty(iconUri))
                    {
                        try
                        {
                            WebClient client = new WebClient();
                            byte[] image = client.DownloadData("http:" + iconUri);
                            using (MemoryStream stream = new MemoryStream(image))
                            {
                                iconBitmap = new Bitmap(stream);
                            }
                        }
                        catch (Exception)
                        {
                            if (!errorShown)
                            {
                                MessageBox.Show("Some images might not be loaded.");
                                errorShown = true; 
                            }
                        }
                    }


                    dt.Rows.Add(country, date, maxtemp, mintemp, maxwindm, maxwindk, humidity, cloud, iconBitmap);
                }

                dataGridView1.DataSource = dt;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            txtcity.Clear();
            txtmaxtemp.Clear();
            txtmintemp.Clear();
            txtwindm.Clear();
            txtwindk.Clear();
            txthumidiity.Clear();
            txtcountry.Clear();
            label7.Text = string.Empty;
            pictureBox1.Image = null; 

            MessageBox.Show("All data cleared successfully.");
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.LightSkyBlue,Color.SteelBlue, 90F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }


    }
}
