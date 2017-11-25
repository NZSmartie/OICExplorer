using ReactiveUI;
using System;

namespace OICExplorer.ViewModels
{
    public class ValueViewModel : ReactiveObject
    {
        public string Name { get; }

        public Type ValueType { get; }

        protected ValueViewModel(string name, Type type)
        {
            Name = name;
            ValueType = type;
        }

        protected object _value;

        public object Value { get => _value; set => _value = value; }
    }

    public class ValueViewModel<T> : ValueViewModel
    {
        public new T Value
        {
            get => (T)_value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        public ValueViewModel(string name, T initialValue = default(T)) 
            : base(name, typeof(T))
        {
            _value = initialValue;
        }
    }
}