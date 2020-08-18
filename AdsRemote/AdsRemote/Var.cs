using AdsRemote.Common;
using System;
using TwinCAT.Ads;

namespace AdsRemote
{
    public class Var<T> : Var
    {
        internal T InternalValue;
        public virtual T RemoteValue
        {
            get => InternalValue;
            set
            {
                InternalValue = value;
                try
                {
                    Device.AdsClient.WriteAny(IndexGroup, IndexOffset, InternalValue); // TODO to refactor
                }
                catch (Exception)
                {
                    // ignored
                }

                if (Device.UiContext != null)
                    Device.UiContext.Send((o) => OnValueChanged(), null);
                else
                    OnValueChanged();
            }
        }

        /// <summary>
        /// Sets internal value without writting to the PLC
        /// </summary>
        /// <param name="value"></param>
        internal override void SetInternalValue(object value)
        {
            InternalValue = (T)value;
            if (Device.UiContext != null)
                Device.UiContext.Send((o) => OnValueChanged(), null);
            else
                OnValueChanged();
        }

        internal Var(long iGroup, long iOffset, AdsDevice adsDevice)
        {
            Device = adsDevice;

            RemoteName = null;
            IndexGroup = iGroup;
            IndexOffset = iOffset;
        }

        internal Var(string name, AdsDevice adsDevice)
        {
            Device = adsDevice;
            RemoteName = name;
            IndexGroup = -1;
            IndexOffset = -1;
        }

        #region Events
        public override event EventHandler<Var> ValueChanged;

        protected override void OnValueChanged()
        {
            EventHandler<Var> eh = ValueChanged;
            eh?.Invoke(this, this);

            OnPropertyChanged("RemoteValue");
        }
        #endregion

        /// <summary>
        /// Trying to unsubscribe from the update events
        /// </summary>
        /// <returns>true - if unsubscribed</returns>
        internal override bool TryUnsubscribe()
        {
            if (!Device.Ready)
                return false;

            try
            {
                if (NotifyHandle > -1)
                    Device.AdsClient.DeleteDeviceNotification(NotifyHandle);
            }
            catch
            {
                NotifyHandle = -1;
            }

            return NotifyHandle == -1;
        }

        /// <summary>
        /// Trying to subscribe to the update events
        /// </summary>
        /// <returns>true - if subscribed</returns>
        internal override bool TrySubscribe()
        {
            if (!Device.Ready)
                return false;

            try
            {
                if (IndexGroup == -1 && IndexOffset == -1)
                    try
                    {
                        ITcAdsSymbol sym = Device.AdsClient.ReadSymbolInfo(RemoteName);
                        IndexGroup = sym.IndexGroup;
                        IndexOffset = sym.IndexOffset;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                if (IndexGroup > -1 && IndexOffset > -1)
                    NotifyHandle =
                        Device.AdsClient.AddDeviceNotificationEx(
                            IndexGroup, IndexOffset,
                            AdsTransMode.OnChange, 0, 0,
                            this,
                            typeof(T));
                else
                    NotifyHandle =
                        Device.AdsClient.AddDeviceNotificationEx(
                            RemoteName,
                            AdsTransMode.OnChange, 0, 0,
                            this,
                            typeof(T));
            }
            catch
            {
                NotifyHandle = -1;
            }

            return NotifyHandle > -1;
        }

        public static implicit operator T(Var<T> var)
        {
            return var.InternalValue;
        }

        public static implicit operator string(Var<T> var)
        {
            return var.ToString();
        }

        public override string ToString()
        {
            return InternalValue == null ? "" : InternalValue.ToString();
        }

        public override object GetValue()
        {
            return InternalValue;
        }

        public override Type ValueType => typeof(T);
    } // class
}
