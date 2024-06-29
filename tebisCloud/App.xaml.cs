using FlyleafLib;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using tebisCloud.Data;
using Config = tebisCloud.Data.Config;

namespace tebisCloud {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static Config Settings { get; private set; }
        private static readonly string _settingsPath;

        static App() {
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        }

        protected override void OnStartup(StartupEventArgs e) {
            if (File.Exists(_settingsPath)) {
                Settings = JsonConvert.DeserializeObject<Config>(File.ReadAllText(_settingsPath)) ?? new Config();
            } else {
                Settings = new Config();
                SaveSettings();
            }

            foreach (var media in Settings.Media) {
                foreach (var part in media.Parts) {
                    part.Parent = media;
                }
            }

            base.OnStartup(e);

            Engine.Start(new EngineConfig() {
                FFmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFmpeg"),
                FFmpegDevices =
                    false, // Prevents loading avdevice/avfilter dll files. Enable it only if you plan to use dshow/gdigrab etc.

#if RELEASE
                FFmpegLogLevel = FFmpegLogLevel.Quiet,
                LogLevel = LogLevel.Quiet,

#else
                FFmpegLogLevel = FFmpegLogLevel.Warning,
                LogLevel = LogLevel.Debug,
                LogOutput = ":debug",
                //LogOutput         = ":console",
                //LogOutput         = @"C:\Flyleaf\Logs\flyleaf.log",                
#endif

                //PluginsPath       = @"C:\Flyleaf\Plugins",

                UIRefresh = false, // Required for Activity, BufferedDuration, Stats in combination with Config.Player.Stats = true
                UIRefreshInterval = 250, // How often (in ms) to notify the UI
                UICurTimePerSecond =
                    true, // Whether to notify UI for CurTime only when it's second changed or by UIRefreshInterval
            });
        }

        public static void SaveSettings() {
            var json = JsonConvert.SerializeObject(Settings);
            File.WriteAllText(_settingsPath, json);
        }
    }
}