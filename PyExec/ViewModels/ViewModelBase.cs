// PyExec/ViewModels/ViewModelBase.cs
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PyExec.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 속성 값을 설정하고 필요한 경우 UI에 변경 사항을 알립니다.
        /// </summary>
        /// <typeparam name="T">속성의 타입입니다.</typeparam>
        /// <param name="field">속성의 기반이 되는 private 필드입니다.</param>
        /// <param name="value">새로운 값입니다.</param>
        /// <param name="propertyName">속성의 이름입니다. 자동으로 채워집니다.</param>
        /// <returns>값이 변경되었으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            // 기존 값과 새 값이 동일하면 아무 작업도 하지 않습니다.
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            // 값을 업데이트하고 변경되었음을 알립니다.
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}