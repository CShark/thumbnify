using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    public class CompressorParam : ParamType, INotifyPropertyChanged {
        private float _preGain = 0;
        private float _threshold = -24;
        private float _knee = 30;
        private float _ratio = 12;
        private float _attack = 3f;
        private float _release = 250f;
        private float _preDelay = 0.006f;
        private float _releaseZone1 = 0.090f;
        private float _releaseZone2 = 0.160f;
        private float _releaseZone3 = 0.420f;
        private float _releaseZone4 = 0.980f;
        private float _postGain = 0;
        private float _wet = 100;
        public event PropertyChangedEventHandler? PropertyChanged;

        public float PreGain {
            get => _preGain;
            set => SetField(ref _preGain, value);
        }

        public float Threshold {
            get => _threshold;
            set => SetField(ref _threshold, value);
        }

        public float Knee {
            get => _knee;
            set => SetField(ref _knee, value);
        }

        public float Ratio {
            get => _ratio;
            set => SetField(ref _ratio, value);
        }

        public float Attack {
            get => _attack;
            set => SetField(ref _attack, value);
        }

        public float Release {
            get => _release;
            set => SetField(ref _release, value);
        }

        public float PreDelay {
            get => _preDelay;
            set => SetField(ref _preDelay, value);
        }

        public float ReleaseZone1 {
            get => _releaseZone1;
            set => SetField(ref _releaseZone1, value);
        }

        public float ReleaseZone2 {
            get => _releaseZone2;
            set => SetField(ref _releaseZone2, value);
        }

        public float ReleaseZone3 {
            get => _releaseZone3;
            set => SetField(ref _releaseZone3, value);
        }

        public float ReleaseZone4 {
            get => _releaseZone4;
            set => SetField(ref _releaseZone4, value);
        }

        public float PostGain {
            get => _postGain;
            set => SetField(ref _postGain, value);
        }

        public float Wet {
            get => _wet;
            set => SetField(ref _wet, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public override ParamType Clone() {
            return new CompressorParam {
                PreGain = PreGain,
                Threshold = Threshold,
                Knee = Knee,
                Ratio = Ratio,
                Attack = Attack,
                Release = Release,
                PreDelay = PreDelay,
                ReleaseZone1 = ReleaseZone1,
                ReleaseZone2 = ReleaseZone2,
                ReleaseZone3 = ReleaseZone3,
                ReleaseZone4 = ReleaseZone4,
                PostGain = PostGain,
                Wet = Wet
            };
        }

        public override void Dispose() {
        }
    }
}