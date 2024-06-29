using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FFmpeg.AutoGen;
using Microsoft.Xaml.Behaviors.Core;

namespace tebisCloud.Postprocessing {
    public class PendingConnection {
        private Connector? _start;

        public ICommand StartCommand { get; }
        public ICommand FinishCommand { get; }

        public PendingConnection(Action<Connector, Connector> createCallback) {
            StartCommand = new ActionCommand(x => _start = x as Connector);
            FinishCommand = new ActionCommand(x => {
                var target = x as Connector;

                if (_start != null && target != null && _start != target) {
                    createCallback(_start, target);
                }
            });
        }
    }
}
