using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using PyExec.Converters; // 추가: GridLengthConverter를 사용하기 위해

namespace PyExec.Models
{
    // JsonConverter for GridLength (This class seems duplicated, but we'll keep it as requested to match the original structure)
    public class GridLengthConverter : JsonConverter<GridLength>
    {
        public override GridLength Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null) return new GridLength(1, GridUnitType.Star);

            // 변경: 전체 네임스페이스를 명시하여 모호성 해결
            return (GridLength)new System.Windows.GridLengthConverter().ConvertFromString(value)!;
        }

        public override void Write(Utf8JsonWriter writer, GridLength value, JsonSerializerOptions options)
        {
            // 변경: 전체 네임스페이스를 명시하여 모호성 해결
            writer.WriteStringValue(new System.Windows.GridLengthConverter().ConvertToString(value));
        }
    }

    public class WindowSettings
    {
        // 외부 창 크기 및 위치
        public double Top { get; set; }
        public double Left { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public WindowState WindowState { get; set; }

        // 내부 패널 높이 (상/중/하 비율)
        // 변경: JsonConverter 타입을 PyExec.Converters 네임스페이스로 명확히 지정
        [JsonConverter(typeof(PyExec.Converters.GridLengthConverter))]
        public GridLength TemplatesPanelHeight { get; set; }

        [JsonConverter(typeof(PyExec.Converters.GridLengthConverter))]
        public GridLength ProgramPanel1Height { get; set; }

        // 각 패널 내부의 너비 (리스트/코드 보기 비율)
        [JsonConverter(typeof(PyExec.Converters.GridLengthConverter))]
        public GridLength Panel1_ListWidth { get; set; }

        [JsonConverter(typeof(PyExec.Converters.GridLengthConverter))]
        public GridLength Panel2_ListWidth { get; set; }
    }
}