using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Events;
using Thumbnify.Data.Processing;

namespace Thumbnify.Tools {
    public class LogMessage {
        public string Message { get; set; }

        public Exception? Exception { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public LogEventLevel Level { get; set; }

        public string? NodeUid { get; set; }

        public string? NodeName { get; set; }

        public string? NodeType { get; set; }

        public LogMessage() {}

        public LogMessage(LogEvent logEvent) {
            Message = logEvent.RenderMessage();
            Timestamp = logEvent.Timestamp;
            Level = logEvent.Level;
            Exception = logEvent.Exception;

            if (logEvent.Properties.ContainsKey("node-uid")) {
                NodeUid = JsonConvert.DeserializeObject<string?>(logEvent.Properties["node-uid"].ToString());
                NodeName = JsonConvert.DeserializeObject<string?>(logEvent.Properties["node-name"].ToString());
                NodeType = JsonConvert.DeserializeObject<string?>(logEvent.Properties["node-type"].ToString());
            }
        }
    }

    public class LocalLogSink : ILogEventSink {
        public ObservableCollection<LogMessage> MessageList { get; } = new();

        public void Emit(LogEvent logEvent) {
            App.Current.Dispatcher.Invoke(() => { MessageList.Add(new LogMessage(logEvent)); });
        }
    }

    public class NodeEnricher : ILogEventEnricher {
        private readonly Node _targetNode;

        public NodeEnricher(Node targetNode) {
            _targetNode = targetNode;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("node-uid", _targetNode.Uid));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("node-type", _targetNode.NodeTypeId));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("node-name", _targetNode.Name));
        }
    }
}