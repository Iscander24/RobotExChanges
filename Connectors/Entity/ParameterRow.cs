using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControllerExChanges.Interfaces;

namespace ControllerExChanges.Entity
{
    [Serializable]
    public class ParameterRow: BaseVM
    {
        public ParameterRow(Type type,
                            string parameterName = "",
                            bool isSecret = false)
        {
            ParameterName = parameterName;
            TypeParameter = type;
            IsSecret = isSecret;
        }
        
        /// <summary>
        /// Отображаемое имя параметра
        /// </summary>
        public string ParameterName
        {
            get => _parameterName;

            set
            {
                _parameterName = value;
                OnPropertyChanged(nameof(ParameterName));
            }
        }
        private string _parameterName = string.Empty;

        /// <summary>
        /// Тип параметра
        /// </summary>
        public Type TypeParameter { get; set; }

        /// <summary>
        /// Значение параметра
        /// </summary>

        public object? Value {get; set; }

        /// <summary>
        /// Нужно ли скрывать символы (пароли)
        /// </summary>
        public bool IsSecret { get; set; } = false;
    }
}
