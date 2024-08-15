using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Thumbnify.Dialogs;

namespace Thumbnify.Data.Processing.Parameters {
    public class YoutubeCredentialsParam : ParamType, INotifyPropertyChanged {
        private string _displayName;
        private string _credentialsUid;

        public string CredentialsUid {
            get => _credentialsUid;
            set {
                if (SetField(ref _credentialsUid, value)) OnPropertyChanged(nameof(CredentialsPath));
            }
        }

        [JsonIgnore]
        public string DisplayName {
            get => _displayName;
            set => SetField(ref _displayName, value);
        }

        [JsonIgnore]
        public string CredentialsPath => Path.Combine(YoutubeCredentials.BasePath, CredentialsUid);

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            if (App.Settings != null) {
                var orig = App.Settings.YoutubeCredentials.FirstOrDefault(x => x.Guid == CredentialsUid);

                if (orig != null) {
                    DisplayName = orig.Name;
                } else {
                    DisplayName = "[missing]";
                }
            }
        }

        public async Task<UserCredential?> ResolveCredentials() {
            var cancelToken = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            if (CredentialsUid != null) {
                try {
                    return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromFile("client_secret.json").Secrets,
                        [
                            YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.YoutubeReadonly,
                            YouTubeService.Scope.Youtube
                        ], "thumbnify", cancelToken.Token,
                        new FileDataStore(CredentialsPath, true));
                } catch {
                    return null;
                }
            } else {
                return null;
            }
        }

        public override ParamType Clone() {
            return this;
        }

        public override void Dispose() {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}