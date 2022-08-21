using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace WebContent2
{
    public partial class Form1 : Form
    {
        private PXWebContentEntities contentDB = new PXWebContentEntities();
        public Form1()
        {
            InitializeComponent();

            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            progressBar1.Step = 1;

            progressBar2.Minimum = 0;
            progressBar2.Value = 0;
            progressBar2.Step = 1;

            progressBar3.Minimum = 0;
            progressBar3.Value = 0;
            progressBar3.Step = 1;
            progressBar3.Maximum = 10;
            progressBar3.Style = ProgressBarStyle.Marquee;


            var lks = contentDB.Tbl_Link.Select(c => c.Link).ToList();
            listBox1.Items.AddRange(lks.ToArray());
            var prxy = contentDB.Tbl_Proxy.Select(c => c.ProxyAdress).ToList();
            listBox5.Items.AddRange(prxy.ToArray());
            prxy = contentDB.Tbl_Proxy.Where(c => c.Status == 1).Select(c => c.ProxyAdress).ToList();
            listBox2.Items.AddRange(prxy.ToArray());
            prxy = contentDB.Tbl_Proxy.Where(c => c.Status == 0).Select(c => c.ProxyAdress).ToList();
            listBox3.Items.AddRange(prxy.ToArray());
            var resps = contentDB.Tbl_HttpRes.ToList();

            foreach (var res in resps.ToArray())
            {
                var pxy = contentDB.Tbl_Proxy.Where(c => c.Id == res.ProxyId).Select(c => c.ProxyAdress).First();
                var lnks = contentDB.Tbl_Link.Where(c => c.Id == res.LinkId).Select(c => c.Link).First();
                var rspss = res.HttpRes;

                listBox4.Items.Add(lnks +  " -> ["  + pxy +  "]"  + rspss);
            }

        }

        private void refreshScreeen()
        {

            contentDB.SaveChanges();

            listBox2.Items.Clear();
            listBox3.Items.Clear();
     

            var prxy = contentDB.Tbl_Proxy.Where(c => c.Status == 1).Select(c => c.ProxyAdress).ToList();
            listBox2.Items.AddRange(prxy.ToArray());
            prxy = contentDB.Tbl_Proxy.Where(c => c.Status == 0).Select(c => c.ProxyAdress).ToList();
            listBox3.Items.AddRange(prxy.ToArray());

        }
        private void refreshResponses()
        {
            contentDB.SaveChanges();
            listBox4.Items.Clear();
            var resps = contentDB.Tbl_HttpRes.ToList();
            foreach (var res in resps.ToArray())
            {
                var pxy = contentDB.Tbl_Proxy.Where(c => c.Id == res.ProxyId).Select(c => c.ProxyAdress).First();
                var lnks = contentDB.Tbl_Link.Where(c => c.Id == res.LinkId).Select(c => c.Link).First();
                var rspss = res.HttpRes;

                listBox4.Items.Add(lnks + " -> [" + pxy + "] " + rspss);
            }

        }

        private void refreshProxy()
        {
            contentDB.SaveChanges();
            listBox5.Items.Clear();
            var prxy = contentDB.Tbl_Proxy.Select(c => c.ProxyAdress).ToList();
            listBox5.Items.AddRange(prxy.ToArray());
        }

        private void refreshLink()

        {
            contentDB.SaveChanges();
            listBox1.Items.Clear();

            var lks = contentDB.Tbl_Link.Select(c => c.Link).ToList();
            listBox1.Items.AddRange(lks.ToArray());

        }

        private void addProxyDB(String prx)
        {

            try
            {
                contentDB.Tbl_Proxy.Where(c => c.ProxyAdress == prx).Select(c => c.Id).First();
            }
            catch (Exception ex)
            {
                var pr = new Tbl_Proxy()
                {
                    ProxyAdress = prx
                };

                contentDB.Tbl_Proxy.Add(pr);
            }
        }

        private void addLinkDB(string lnk)
        {
            try
            {
                contentDB.Tbl_Link.Where(c => c.Link == lnk).Select(c => c.Id).First();

            }
            catch (Exception ex)
            {
                var lk = new Tbl_Link()
                {
                    Link = lnk
                };

                contentDB.Tbl_Link.Add(lk);
            }

        }

        private void addHttpResDB(string resp, string prx, string lnk)
        {
            var res = new Tbl_HttpRes()
            {
                HttpRes = resp
            };

            res.ProxyId = contentDB.Tbl_Proxy
                     .Where(c => c.ProxyAdress == prx)
                     .Select(c => c.Id).First();
            res.LinkId = contentDB.Tbl_Link
                     .Where(c => c.Link == lnk)
                     .Select(c => c.Id).First();

            contentDB.Tbl_HttpRes.Add(res);
        }


        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        async Task proxyPrepare()
        {
            await Task.Run(() =>
            {
                foreach (string s in listBox5.Items)
                {
                    var name = s;
                    strIP = s.Split(':')[0];
                    intPort = Convert.ToInt32(s.Split(':')[1]);
                    bool tt = PingHost(strIP, intPort);
                    this.Invoke(new MethodInvoker(delegate ()
                    {

                        progressBar1.PerformStep();

                    }));

                    if (tt == true)
                    {
                        this.Invoke(new MethodInvoker(delegate ()
                        {

                            var prr = contentDB.Tbl_Proxy.Where(c => c.ProxyAdress == s).First();
                            prr.Status = 1;
                            refreshScreeen();


                        }));


                    }
                    else
                    {

                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            var prr = contentDB.Tbl_Proxy.Where(c => c.ProxyAdress == s).First();
                            prr.Status = 0;
                            refreshScreeen();

                        }));
                    }
                }

            });
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Proxy Check Started ...");
            progressBar1.Visible = true;
            await proxyPrepare();
            MessageBox.Show("Proxy Check done ...");
        }



        string strIP = "10.0.0.0";
        int intPort = 12345;

        public static bool PingHost(string strIP, int intPort)
        {

            try
            {

                TcpClient tcpClient = new TcpClient();
                if (tcpClient.ConnectAsync(strIP, intPort).Wait(TimeSpan.FromSeconds(3)))
                {
                    NetworkStream nStream = tcpClient.GetStream();

                    return true;
                }
                else
                {

                    return false;
                }


            }
            catch (Exception ex)
            {

                return false;
            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Proxy loading .....");
            progressBar3.Visible = true;
            string html = getProxyIPs();
            //richTextBox1.Text = html;
            progressBar3.Visible = false;
            MessageBox.Show("Proxy finish ..");
            // progressBar3.PerformStep();
            progressBar1.Maximum = listBox5.Items.Count;
        }

        private static string GetHttpContent(string uri, CookieContainer container)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = container,
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18363");
            client.Timeout = TimeSpan.FromMinutes(1.0);
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };
            HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
            HttpContent content = response.Content;
            byte[] buffer = content.ReadAsByteArrayAsync().GetAwaiter().GetResult().ToArray();
            string html = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return WebUtility.HtmlDecode(html);
        }



        private string getProxyIPs()
        {
            string[] UrlList = { "https://spys.one/" }; //, "https://free-proxy-list.net/" , "https://free-proxy-list.net/anonymous-proxy.html", "https://www.sslproxies.org/", "https://www.socks-proxy.net/", "https://www.us-proxy.org/", "https://free-proxy-list.net/uk-proxy.html" };
            string sonuc = "";
            foreach (string Url in UrlList)
            {

                string html = GetHttpContent(Url, new CookieContainer());
                string pattern = @"\d+\.\d+\.\d+\.\d+:\d+";

                try
                {
                    foreach (Match match in Regex.Matches(html, pattern, RegexOptions.IgnoreCase))
                    {
                        sonuc += match.Value + "\n";
                        //listBox5.Items.Add(match.Value);
                        addProxyDB(match.Value);
                    }
                    refreshProxy();
                }
                catch (RegexMatchTimeoutException) { }
            }

            return sonuc;

        }

        private static string GetHttpContent(string uri, CookieContainer container, string px)
        {
            // First create a proxy object
            WebProxy proxy;
            proxy = new WebProxy

            {
                Address = new Uri($"http://{px}")
            };


            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = container,
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                Proxy = proxy,
                UseProxy = true,
            };

            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18363");
            client.Timeout = TimeSpan.FromMinutes(1.0);
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };
            HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
            HttpContent content = response.Content;
            byte[] buffer = content.ReadAsByteArrayAsync().GetAwaiter().GetResult().ToArray();
            string html = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return WebUtility.HtmlDecode(html);
        }


        private void btn_Linkekle_Click(object sender, EventArgs e)
        {
            progressBar2.Maximum = listBox1.Items.Count;


            if (textBox1.Text != "")
            {


                //listBox1.Items.Add(textBox1.Text);


                addLinkDB(textBox1.Text);
                refreshLink();
               
            }

            
        }

        async Task httpRequest(string selectedprx)
        {
            //string selectedprx = listBox2.SelectedItem.ToString();

            await Task.Run(() =>
            {

                foreach (string link in listBox1.Items)
                {

                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        try
                        {
                            var resp = GetHttpContent(link, new CookieContainer(), selectedprx);

                            //listBox4.Items.Add(resp);
                            addHttpResDB(resp, selectedprx, link);
                            refreshResponses();
                        }
                        catch (Exception ex)
                        {

                            //listBox4.Items.Add($"{ex.Message}");
                            addHttpResDB(ex.Message, selectedprx, link);
                            refreshResponses();
                        }
                    }));

                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        progressBar2.PerformStep();

                    }));
                }
            });
        }


        private async void button3_Click(object sender, EventArgs e)
        {

            progressBar2.Visible = true;


            try
            {

                string selectedprx = listBox2.SelectedItem.ToString();
                await httpRequest(selectedprx);
            }
            catch (Exception ex)
            {


            }
            MessageBox.Show("Finish..");

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

}




