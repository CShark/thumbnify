using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using Thumbnify.Data.Processing.Audio;
using Thumbnify.Data.Processing.Parameters;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für CompressorPreview.xaml
    /// </summary>
    public partial class CompressorPreview : UserControl {
        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(
            nameof(Parameters), typeof(CompressorParam), typeof(CompressorPreview), new PropertyMetadata(
                default(CompressorParam?),
                (o, e) => {
                    if (e.OldValue is CompressorParam paramOld) {
                        paramOld.PropertyChanged -= ((CompressorPreview)o).ParametersOnPropertyChanged;
                        ((CompressorPreview)o).Curve = null;
                    }

                    if (e.NewValue is CompressorParam paramNew) {
                        paramNew.PropertyChanged += ((CompressorPreview)o).ParametersOnPropertyChanged;
                        ((CompressorPreview)o).ParametersOnPropertyChanged(null, null);
                    }
                }));

        public CompressorParam? Parameters {
            get { return (CompressorParam?)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        public static readonly DependencyProperty CurveProperty = DependencyProperty.Register(
            nameof(Curve), typeof(PathGeometry), typeof(CompressorPreview),
            new PropertyMetadata(default(PathGeometry)));

        public PathGeometry Curve {
            get { return (PathGeometry)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }

        public static readonly DependencyProperty MinDbProperty = DependencyProperty.Register(
            nameof(MinDb), typeof(float), typeof(CompressorPreview), new PropertyMetadata(-60f));

        public float MinDb {
            get { return (float)GetValue(MinDbProperty); }
            set { SetValue(MinDbProperty, value); }
        }

        public static readonly DependencyProperty MaxDbProperty = DependencyProperty.Register(
            nameof(MaxDb), typeof(float), typeof(CompressorPreview), new PropertyMetadata(20f));

        public float MaxDb {
            get { return (float)GetValue(MaxDbProperty); }
            set { SetValue(MaxDbProperty, value); }
        }

        public CompressorPreview() {
            InitializeComponent();

            if (Parameters != null) {
                Parameters.PropertyChanged += ParametersOnPropertyChanged;
                ParametersOnPropertyChanged(null, null);
            }
        }

        private void ParametersOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (Parameters == null) return;

            var curve = MakeCurve(Parameters.Threshold, Parameters.Knee, Parameters.Ratio);

            var geometry = new PathGeometry();
            var figure = new PathFigure();

            figure.StartPoint = new Point(MinDb, MinDb);

            for (float db=MinDb; db <= MaxDb; db++) {
                var y = curve(db);

                figure.Segments.Add(new LineSegment(new Point(db, y), true));
            }

            figure.Segments.Add(new LineSegment(new Point(MaxDb, MaxDb), false));

            geometry.Figures.Add(figure);
            Curve = geometry;
        }

        private Func<float, float> MakeCurve(float threshold, float knee, float ratio) {
            var slope = 1 / ratio;

            if (knee <= 0) {
                return db => {
                    if (db < threshold) {
                        return db;
                    }

                    return threshold + slope * (db - threshold);
                };
            }

            var linearThreshold = AudioCompressor.Db2Lin(threshold);
            var k = 5f;
            var xknee = AudioCompressor.Db2Lin(threshold + knee);
            var mink = 0.1f;
            var maxk = 10000f;
            for (var i = 0; i < 15; i++) {
                if (AudioCompressor.KneeSlope(xknee, k, linearThreshold) < slope) {
                    maxk = k;
                } else {
                    mink = k;
                }

                k = (float)Math.Sqrt(mink * maxk);
            }

            var kneeBOffset = AudioCompressor.Lin2Db(AudioCompressor.KneeCurve(xknee, k, linearThreshold));

            return db => {
                if (db < threshold) {
                    return db;
                }

                if (db < threshold + knee) {
                    return AudioCompressor.Lin2Db(AudioCompressor.KneeCurve(AudioCompressor.Db2Lin(db), k,
                        linearThreshold));
                }

                return kneeBOffset + slope * (db - threshold - knee);
            };
        }
    }
}