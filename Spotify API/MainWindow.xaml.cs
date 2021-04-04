using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spotify_API
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Grid grid = new Grid();
            StackPanel panel = new StackPanel();
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            TextBlock textBlock = new TextBlock();
           
           
            panel.Margin = new Thickness(0, 60, 0, 0);
            panel.Orientation = Orientation.Vertical;
            grid.Margin = new Thickness(10, 10, 10, 10);
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Bottom;
            textBlock.Foreground = Brushes.Gray;
            textBlock.Margin = new Thickness(59, 0, 59, 0);
            textBlock.Text = "auernig";
            image.Width = 150;
            image.Height = 150;

            Uri resourceUri = new Uri("https://www.google.at/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png", UriKind.RelativeOrAbsolute);
            image.Source = new BitmapImage(resourceUri);

            panel.Children.Add(image);
            panel.Children.Add(textBlock);
            grid.Children.Add(panel);

            cover.Children.Add(grid);

            //SpotifyPlaylists playlists = GetPlayLists(GetToken(), userid.Text);
        }

        private static SpotifyPlaylists GetPlayLists(string token, string user)
        {
            string url = string.Format("https://api.spotify.com/v1/users/{0}/playlists", user);
            SpotifyPlaylists playLists = JsonConvert.DeserializeObject<SpotifyPlaylists>(GetSpotifyType(token, url));
            return playLists;
        }

        private static SpotifyTracks GetTracks(string token, string playlistid)
        {
            string url = string.Format("https://api.spotify.com/v1/playlists/{0}/tracks", playlistid);
            SpotifyTracks tracks = JsonConvert.DeserializeObject<SpotifyTracks>(GetSpotifyType(token, url));
            return tracks;
        }

        private static string GetSpotifyType(string token, string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + token);
                request.ContentType = "application/json; charset=utf-8";

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string GetToken()
        {
            string clientsecret = "8fd74bacb679414e903a9fd81c715907";
            string clientname = "92ade0a726374eb1b29885752a8b5e1a";
            string token;

            string postString = string.Format("grant_type=client_credentials");

            byte[] byteArray = Encoding.UTF8.GetBytes(postString);
            string url = "https://accounts.spotify.com/api/token";

            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            var authheader = Convert.ToBase64String(Encoding.Default.GetBytes($"{clientname}:{clientsecret}"));

            request.Headers.Add("Authorization", "Basic " + authheader); request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = byteArray.Length;

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            string responseFromServer = reader.ReadToEnd();
                            JObject obj = JObject.Parse(responseFromServer);
                            token = (string)obj.SelectToken("access_token");
                        }
                    }
                }
            }
            return token;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
