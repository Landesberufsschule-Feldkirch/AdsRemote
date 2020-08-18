using System;
using System.ComponentModel;

namespace AdsRemote.Common
{
    public abstract class Var : IDisposable, INotifyPropertyChanged
    {
        public string Name => RemoteName;
        protected int NotifyHandle = -1;

        internal AdsDevice Device;
        internal string RemoteName = null;
        internal long IndexGroup = -1;
        internal long IndexOffset = -1;

        internal abstract bool TrySubscribe();
        internal abstract bool TryUnsubscribe();
        internal abstract void SetInternalValue(object value);
        public abstract object GetValue();
        public abstract Type ValueType { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public abstract event EventHandler<Var> ValueChanged;
        protected abstract void OnValueChanged();

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler pceh = PropertyChanged;
            pceh?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                TryUnsubscribe();
        }
    }
}
