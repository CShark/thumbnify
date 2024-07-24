﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;
using tebisCloud.Data.Processing;

namespace tebisCloud.Data {
    public class LogMessage {
        public string Message { get; }

        public DateTimeOffset Timestamp { get; }

        public LogEventLevel Level { get; }

        public string? NodeUid { get; }

        public LogMessage(LogEvent logEvent) {
            Message = logEvent.RenderMessage();
            Timestamp = logEvent.Timestamp;
            Level = logEvent.Level;

            if (logEvent.Properties.ContainsKey("node-uid")) {
                NodeUid = logEvent.Properties["node-uid"].ToString();
            }
        }
    }

    public class LocalLogSink : ILogEventSink {
        public ObservableCollection<LogMessage> MessageList { get; } = new();

        public void Emit(LogEvent logEvent) {
            App.Current.Dispatcher.Invoke(() => {
                MessageList.Add(new LogMessage(logEvent));
            });
        }
    }

    public class NodeEnricher : ILogEventEnricher {
        private readonly Node _targetNode;

        public NodeEnricher(Node targetNode) {
            _targetNode = targetNode;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("node-uid", _targetNode.Uid));
        }
    }
}