using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
        }

        private async Task<SpotifyPlaylists> GetPlayLists(string token, string user)
        {
            string url = string.Format("https://api.spotify.com/v1/users/{0}/playlists", user.Trim());
            SpotifyPlaylists playLists = JsonConvert.DeserializeObject<SpotifyPlaylists>(await GetSpotifyType(token, url));
            url = playLists.next;

            if (playLists.total > 20)
            {
                do
                {
                    SpotifyPlaylists tmp = JsonConvert.DeserializeObject<SpotifyPlaylists>(await GetSpotifyType(token, url));
                    if (tmp.next != null)
                    {
                        url = tmp.next;
                    }
                    else
                    {
                        url = null;
                    }

                    for (int i = 0; i < tmp.items.Count; i++)
                    {
                        playLists.items.Add(tmp.items[i]);
                    }
                } while (url != null);
            }

            return playLists;
        }

        private async Task<SpotifyTracks> GetTracks(string token, string playlistid)
        {
            string url = string.Format("https://api.spotify.com/v1/playlists/{0}/tracks", playlistid);
            SpotifyTracks tracks = JsonConvert.DeserializeObject<SpotifyTracks>(await GetSpotifyType(token, url));

            if (tracks.total > 20)
            {
                do
                {
                    SpotifyTracks tmp = JsonConvert.DeserializeObject<SpotifyTracks>(await GetSpotifyType(token, url));
                    if (tmp.next != null)
                    {
                        url = tmp.next.ToString();
                    }
                    else
                    {
                        url = null;
                    }
                        

                    for (int i = 0; i < tmp.items.Count; i++)
                    {
                        tracks.items.Add(tmp.items[i]);
                    }
                } while (url != null);
            }

            return tracks;
        }

        private async Task<string> GetSpotifyType(string token, string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + token);
                request.ContentType = "application/json; charset=utf-8";

                using (WebResponse response = await request.GetResponseAsync())
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
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<string> GetToken()
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
                using (WebResponse response = await request.GetResponseAsync())
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            cover.Children.RemoveRange(0, cover.Children.Count);
            errorbox.Text = "";

            if (userid.Text == "")
            {
                errorbox.Text = "insert a userid";
                return;
            }

            SpotifyPlaylists playlists = new SpotifyPlaylists();

            try
            {
                playlists = await GetPlayLists(await GetToken(), userid.Text);
            }
            catch (Exception)
            {

                errorbox.Text = "Something went wrong";
            }
           

            int tracks = 0;
            for (int i = 0; i < playlists.items.Count; i++)
            {
                tracks += playlists.items[i].tracks.total;
            }

            playlistcount.Text = "Playlists: " + playlists.items.Count;
            songcount.Text = "Songs: " + tracks;

            if (track.Text != "")
            {
                for (int i = 0; i < playlists.items.Count; i++)
                {
                    bool containstrack = false;
                    SpotifyTracks playlisttracks = await GetTracks(await GetToken(), playlists.items[i].id);

                    for (int y = 0; y < playlisttracks.items.Count; y++)
                    {
                        if (playlisttracks.items[y].track.name.ToLower() == track.Text.ToLower())
                        {
                            containstrack = true;
                        }
                    }

                    if (!containstrack)
                    {
                        playlists.items.RemoveAt(i);
                        i--;
                    }
                }
            }

            for (int i = 0; i < playlists.items.Count; i++)
            {
                Grid grid = new Grid();
                StackPanel panel = new StackPanel();
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                TextBlock textBlock = new TextBlock();
                DropShadowEffect myDropShadowEffect = new DropShadowEffect();

                myDropShadowEffect.Color = Colors.Black;
                myDropShadowEffect.Direction = 320;
                myDropShadowEffect.ShadowDepth = 25;
                myDropShadowEffect.Opacity = 0.5;
                myDropShadowEffect.BlurRadius = 40;
                panel.Margin = new Thickness(0, 60, 0, 0);
                panel.Orientation = Orientation.Vertical;
                grid.Margin = new Thickness(10, 10, 10, 10);
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock.Foreground = Brushes.Gray;
                textBlock.Margin = new Thickness(59, 5, 59, 0);
                textBlock.Width = 100;
                textBlock.TextAlignment = TextAlignment.Center;
                image.Width = 150;
                image.Height = 150;

                textBlock.Text = playlists.items[i].name;

                Uri resourceUri = new Uri(playlists.items[i].images[0].url, UriKind.RelativeOrAbsolute);
                image.Source = new BitmapImage(resourceUri);

                image.Effect = myDropShadowEffect;
                panel.Children.Add(image);
                panel.Children.Add(textBlock);
                grid.Children.Add(panel);

                cover.Children.Add(grid);
            }
        }
    }
}
