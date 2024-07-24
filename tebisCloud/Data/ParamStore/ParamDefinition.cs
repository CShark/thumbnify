using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Material.Icons;
using Thumbnify.Data.Processing;
using Thumbnify.Data.Processing.Parameters;

namespace Thumbnify.Data.ParamStore {
    public class ParamGenerator {
        public string Id { get; set; }

        public MaterialIconKind Icon { get; set; }

        public Func<ParamDefinition> CreateNew { get; set; }

        public ParamGenerator(string id, MaterialIconKind icon, Func<ParamDefinition> createNew) {
            Id = id;
            Icon = icon;
            CreateNew = createNew;
        }
    }

    public class ParamDefinition {
        public static IReadOnlyList<ParamGenerator> SupportedTypes = new List<ParamGenerator> {
            new("input_text", MaterialIconKind.FormTextbox, () => new ParamDefinition {
                Type = typeof(StringParam),
                Value = new StringParam()
            }),
            new("input_date", MaterialIconKind.Calendar, () => new ParamDefinition {
                Type = typeof(DateParam),
                Value = new DateParam()
            }),
            new("input_thumbnail", MaterialIconKind.ImageArea, () => new ParamDefinition {
                Type = typeof(ThumbnailParam),
                Value = new ThumbnailParam()
            })
        };

        public string Id { get; set; }

        public string Name { get; set; }

        public ParamType Value { get; set; }

        public Type Type { get; set; }

        public ParamDefinition Clone() {
            return new ParamDefinition { Id = Id, Name = Name, Value = Value.Clone() };
        }

        public Result? BuildResult() {
            switch (Value) {
                case StringParam strParam:
                    return new Result<StringParam>(Id) {
                        Value = strParam,
                        Name = Name
                    };
                case ThumbnailParam thumbParam:
                    return new Result<ThumbnailParam>(Id) {
                        Value = thumbParam,
                        Name = Name
                    };
                case DateParam dateParam:
                    return new Result<DateParam>(Id) {
                        Value = dateParam,
                        Name = Name
                    };
                default:
                    return null;
            }
        }
    }
}