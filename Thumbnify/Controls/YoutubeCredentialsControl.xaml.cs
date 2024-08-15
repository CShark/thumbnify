using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Thumbnify.Data;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Dialogs;
using MessageBox = Thumbnify.Dialogs.MessageBox;
using Path = System.IO.Path;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für YoutubeCredentialsControl.xaml
    /// </summary>
    public partial class YoutubeCredentialsControl : UserControl {
        public static readonly DependencyProperty CredentialsProperty = DependencyProperty.Register(
            nameof(Credentials), typeof(YoutubeCredentialsParam), typeof(YoutubeCredentialsControl),
            new PropertyMetadata(default(YoutubeCredentialsParam)));

        public YoutubeCredentialsParam Credentials {
            get { return (YoutubeCredentialsParam)GetValue(CredentialsProperty); }
            set { SetValue(CredentialsProperty, value); }
        }

        public event Action CredentialsChanged;

        public YoutubeCredentialsControl() {
            InitializeComponent();
        }

        private void SelectCredentials_OnClick(object sender, RoutedEventArgs e) {
            var cancelToken = new CancellationTokenSource();

            var result =
                LoadSaveDialog.ShowOpenDialog(Window.GetWindow(this), App.Settings.YoutubeCredentials, x => {
                    App.Settings.YoutubeCredentials.Remove(x);
                    Directory.Delete(Credentials.CredentialsPath);
                }, async () => {
                    var guid = Guid.NewGuid().ToString();
                    var credentials = new YoutubeCredentials();
                    UserCredential? cred = null;

                    try {
                        cred = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromFile("client_secret.json").Secrets,
                            [
                                YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.YoutubeReadonly,
                                YouTubeService.Scope.Youtube
                            ], "thumbnify", cancelToken.Token,
                            new FileDataStore(Path.Combine(YoutubeCredentials.BasePath, guid), true));
                    } catch { }

                    if (cred != null) {
                        var service = new YouTubeService(new BaseClientService.Initializer {
                            HttpClientInitializer = cred,
                            ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
                        });

                        var req = service.Channels.List(new[] { "snippet" });
                        req.Mine = true;
                        var channels = await req.ExecuteAsync();

                        if (channels.Items != null && channels.Items.Count > 0) {
                            credentials.Guid = guid;
                            credentials.Name = channels.Items[0].Snippet.Title;

                            using (var client = new HttpClient())
                            using (var stream =
                                   await client.GetStreamAsync(channels.Items[0].Snippet.Thumbnails.High.Url))
                            using (var file = new FileStream(
                                       Path.Combine(YoutubeCredentials.BasePath, guid, "thumbnail.png"),
                                       FileMode.Create)) {
                                stream.CopyTo(file);
                            }

                            credentials.OnDeserialized(default);
                            App.Settings.YoutubeCredentials.Add(credentials);

                            Credentials.CredentialsUid = guid;
                            Credentials.DisplayName = credentials.Name;

                            return true;
                        } else {
                            MessageBox.ShowDialog(Window.GetWindow(this), "ytNoChannels", MessageBoxButton.OK);
                        }
                    }

                    Directory.Delete(Path.Combine(YoutubeCredentials.BasePath, guid), true);

                    return false;
                });

            cancelToken.Cancel();

            if (result != null) {
                Credentials.CredentialsUid = result.Guid;
                Credentials.DisplayName = result.Name;
                OnCredentialsChanged();
            }
        }

        protected virtual void OnCredentialsChanged() {
            CredentialsChanged?.Invoke();
        }
    }
}