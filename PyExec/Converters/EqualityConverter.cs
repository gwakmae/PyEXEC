using System;
using System.Globalization;
using System.Windows.Data;

namespace PyExec.Converters
{
    public class EqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0]는 현재 Border의 ViewModel (예: Panel1VM)
            // values[1]은 전체 Window의 활성 ViewModel (ActivePanelVM)
            // 두 참조가 동일한 객체를 가리키는지 확인하여 true/false 반환
            if (values.Length >= 2)
            {
                return object.ReferenceEquals(values[0], values[1]);
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}