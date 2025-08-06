// PyExec/Converters/FileContentConverter.cs
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace PyExec.Converters
{
    public class FileContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 파일 경로가 유효한 문자열인지 확인합니다.
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    // 파일이 실제로 존재하는지 확인합니다.
                    if (File.Exists(path))
                    {
                        // 실행 파일은 내용을 표시할 수 없음을 알립니다.
                        if (Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            return "실행 파일(.exe)은 소스 코드를 표시할 수 없습니다.";
                        }
                        // 파일 내용을 읽어 반환합니다.
                        return File.ReadAllText(path);
                    }
                    else
                    {
                        return "파일을 찾을 수 없습니다.";
                    }
                }
                catch (Exception ex)
                {
                    // 파일 읽기 중 오류가 발생하면 메시지를 반환합니다.
                    return $"파일을 읽는 중 오류가 발생했습니다: {ex.Message}";
                }
            }
            // 유효한 경로가 아니면 기본 메시지를 반환합니다.
            return "프로그램을 선택하여 내용을 확인하세요.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 이 변환기는 단방향(OneWay)으로만 사용되므로 ConvertBack은 구현하지 않습니다.
            throw new NotImplementedException();
        }
    }
}