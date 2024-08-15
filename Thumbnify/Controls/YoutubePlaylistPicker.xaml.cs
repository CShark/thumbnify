﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Thumbnify.Data.Processing.Parameters;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für YoutubePlaylistPicker.xaml
    /// </summary>
    public partial class YoutubePlaylistPicker : UserControl {
        public record Item(string Id, string Name);

        public static readonly DependencyProperty PlaylistProperty = DependencyProperty.Register(
            nameof(Playlist), typeof(YoutubePlaylistParam), typeof(YoutubePlaylistPicker),
            new PropertyMetadata(default(YoutubePlaylistParam)));

        public YoutubePlaylistParam Playlist {
            get { return (YoutubePlaylistParam)GetValue(PlaylistProperty); }
            set { SetValue(PlaylistProperty, value); }
        }

        public static readonly DependencyProperty PlaylistItemsProperty = DependencyProperty.Register(
            nameof(PlaylistItems), typeof(ObservableCollection<Item>), typeof(YoutubePlaylistPicker),
            new PropertyMetadata(default(ObservableCollection<Item>)));

        public ObservableCollection<Item> PlaylistItems {
            get { return (ObservableCollection<Item>)GetValue(PlaylistItemsProperty); }
            set { SetValue(PlaylistItemsProperty, value); }
        }


        public YoutubePlaylistPicker() {
            PlaylistItems = new();

            InitializeComponent();
        }

        private async void YoutubeCredentialsControl_OnCredentialsChanged() {
            Playlist.PlaylistId = "";
            PlaylistItems.Clear();

            var cred = await Playlist.Credentials.ResolveCredentials();

            if (cred != null) {
                var service = new YouTubeService(new BaseClientService.Initializer {
                    HttpClientInitializer = cred,
                    ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
                });

                var req = service.Playlists.List("snippet");
                req.Mine = true;

                var lists = await req.ExecuteAsync();

                foreach (var list in lists.Items) {
                    PlaylistItems.Add(new Item(list.Id, list.Snippet.Title));
                }
            }
        }
    }
}