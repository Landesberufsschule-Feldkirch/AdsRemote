using AdsRemote.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using TwinCAT.Ads;

namespace AdsRemote
{
    public class Plc : IDisposable, INotifyPropertyChanged
    {
        public readonly AmsRouter Router;

        private readonly Dictionary<int, AdsDevice> _dictPortDevice = new Dictionary<int, AdsDevice>();
        private readonly object _lockerDictPortDevice = new object();
        private readonly Dictionary<string, Var> _dictNameVar = new Dictionary<string, Var>();

        Thread _pingThread;
        CancellationTokenSource _cancelTokenSource;
        SynchronizationContext _uiContext;  // TODO to refactor

        #region Default Runtimes
        // ReSharper disable once UnusedMember.Global
        public AdsDevice Runtime1
        {
            get
            {
                _dictPortDevice.TryGetValue((int)AmsPort3.PlcRuntime1, out var d);
                return d;
            }
        }
        // ReSharper disable once UnusedMember.Global
        public AdsDevice Runtime2
        {
            get
            {
                _dictPortDevice.TryGetValue((int)AmsPort3.PlcRuntime2, out var d);
                return d;
            }
        }
        // ReSharper disable once UnusedMember.Global
        public AdsDevice Runtime3
        {
            get
            {
                _dictPortDevice.TryGetValue((int)AmsPort3.PlcRuntime3, out var d);
                return d;
            }
        }
        // ReSharper disable once UnusedMember.Global
        public AdsDevice Runtime4
        {
            get
            {
                _dictPortDevice.TryGetValue((int)AmsPort3.PlcRuntime4, out var d);
                return d;
            }
        }
        #endregion

        public int TunePingSleepInterval = 1;      // How long ping thread sleeps between iterations
        public int TuneReinitInterval = 100;       // Interval after connection but before vars subscription

        private int _tuneAdsClientTimeout = 1000;   // I/O operations Meintimeout
        // ReSharper disable once UnusedMember.Global
        public int TuneAdsClientTimeout
        {
            get => _tuneAdsClientTimeout;

            set
            {
                _tuneAdsClientTimeout = value;

                List<AdsDevice> devices = new List<AdsDevice>();
                lock (_lockerDictPortDevice)
                {
                    if (devices.Count != _dictPortDevice.Values.Count)
                        devices = new List<AdsDevice>(_dictPortDevice.Values);
                }

                foreach (AdsDevice device in devices)
                    device.AdsClient.Timeout = _tuneAdsClientTimeout;
            } // set
        }

        #region ctor
        // ReSharper disable once UnusedMember.Global
        public Plc(AmsNetId amsNetId)
        {
            Router = new AmsRouter(amsNetId);
            ctor_initialize();
        }

        public Plc(string amsNetId)
        {
            Router = new AmsRouter(new AmsNetId(amsNetId));
            ctor_initialize();
        }

        private void ctor_initialize()
        {
            _cancelTokenSource = new CancellationTokenSource();
            SynchronizationContext uiContext = SynchronizationContext.Current;
            _pingThread = new Thread(() => PingThread(_cancelTokenSource.Token, uiContext)) {IsBackground = true};
            _pingThread.Start();
        }
        #endregion

        private void PingThread(CancellationToken token, SynchronizationContext uiContext)
        {
            _uiContext = uiContext;
            List<AdsDevice> devices = new List<AdsDevice>();
            List<AdsDevice> updateList = new List<AdsDevice>();
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                lock (_lockerDictPortDevice)
                {
                    if (devices.Count != _dictPortDevice.Values.Count)
                        devices = new List<AdsDevice>(_dictPortDevice.Values);
                }

                foreach (AdsDevice device in devices)
                {
                    if (token.IsCancellationRequested)
                        break;

                    if (device.Vars.Count > 0)
                    {
                        bool isActive = false;
                        try
                        {
                            Var v = device.Vars[0];
                            if (v.IndexGroup > -1 && v.IndexOffset > -1)
                                device.AdsClient.ReadAny(v.IndexGroup, v.IndexOffset, v.ValueType);
                            else
                                device.AdsClient.ReadSymbol(v.Name, v.ValueType, !device.Ready);

                            isActive = true;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (isActive && (!device.Ready) || (!isActive) && device.Ready)
                            updateList.Add(device);
                    } // foreach

                    if (token.IsCancellationRequested)
                        break;
                } // foreach (AdsDevice device in devices)

                if (!token.IsCancellationRequested)
                    Thread.Sleep(updateList.Count > 0 ? TuneReinitInterval : TunePingSleepInterval);

                foreach (AdsDevice device in updateList)
                {
                    if (token.IsCancellationRequested)
                        break;

                    if (!device.Ready)
                    {
                        device.SetActive(true);

                        if (uiContext != null)
                            uiContext.Send(OnDeviceReady, device);
                        else
                            OnDeviceReady(device);
                    }
                    else
                    {
                        device.SetActive(false);

                        if (uiContext != null)
                            uiContext.Send(OnDeviceLost, device);
                        else
                            OnDeviceLost(device);
                    }

                    if (token.IsCancellationRequested)
                        break;
                } // foreach

                if (!token.IsCancellationRequested)
                    updateList.Clear();
            } // while(true)
        } // PingThreadMethod(...

        #region Events
        public event EventHandler<AdsDevice> DeviceReady;
        public event EventHandler<AdsDevice> DeviceLost;
        public event PropertyChangedEventHandler PropertyChanged;

        // ReSharper disable once UnusedMember.Global
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler pceh = PropertyChanged;
            pceh?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnDeviceLost(object objDevice)
        {
            AdsDevice device = (AdsDevice)objDevice;

            foreach (Var v in device.Vars)
                v.TryUnsubscribe();

            EventHandler<AdsDevice> handle = DeviceLost;
            handle?.Invoke(this, device);
        }

        protected virtual void OnDeviceReady(object objDevice)
        {
            AdsDevice device = (AdsDevice)objDevice;

            foreach (Var var in device.Vars)
            {
                var.TryUnsubscribe();
                var.TrySubscribe();
            }

            EventHandler<AdsDevice> handle = DeviceReady;
            handle?.Invoke(this, device);
        }

        private void Client_AdsNotificationEx(object sender, AdsNotificationExEventArgs e)
        {
            Var v = (Var)e.UserData;
            v.SetInternalValue(e.Value);
        }
        #endregion

        #region PLC Variables and Class
        private AdsDevice GetDevice(int port)
        {
            if (!_dictPortDevice.TryGetValue(port, out var device))
            {
                device = new AdsDevice(Router.AmsNetId, port);
                device.AdsClient.Timeout = _tuneAdsClientTimeout;
                device.AdsClient.AdsNotificationEx += Client_AdsNotificationEx;
                device.UiContext = _uiContext;

                lock (_lockerDictPortDevice)
                    _dictPortDevice.Add(port, device);
            }

            return device;
        }

        private Var<T> CreateVariable<T>(string varName, int port)
        {
            if (_dictNameVar.TryGetValue(varName, out var v))
                return (Var<T>)v;

            AdsDevice device = GetDevice(port);

            Var<T> var = new Var<T>(varName, device);
            device.Vars.Add(var);
            _dictNameVar.Add(varName, var);

            var.TrySubscribe();

            return var;
        }

        private Var<T> CreateVariable<T>(long grp, long offs, int port)
        {
            string pseudoName = string.Concat(grp.ToString(), ":", offs.ToString());

            if (_dictNameVar.TryGetValue(pseudoName, out var v))
                return (Var<T>)v;

            AdsDevice device = GetDevice(port);

            Var<T> var = new Var<T>(grp, offs, device);
            device.Vars.Add(var);
            _dictNameVar.Add(pseudoName, var);

            var.TrySubscribe();

            return var;
        }

        public Var<T> Var<T>(string variable, int port)
        {
            return CreateVariable<T>(variable, port);
        }

        // ReSharper disable once UnusedMember.Global
        public Var<T> Var<T>(long grp, long offs, int port)
        {
            return CreateVariable<T>(grp, offs, port);
        }

        // ReSharper disable once UnusedMember.Global
        public Var<T> Var<T>(string variable)
        {
            return Var<T>(variable, (int)AmsPort3.PlcRuntime1);
        }

        // ReSharper disable once UnusedMember.Global
        public Var<T> Var<T>(string variable, AmsPort3 port)
        {
            return Var<T>(variable, (int)port);
        }

        public T Class<T>(T instance = default) where T : new()
        {
            T o =
                instance == null ?
                new T() :
                instance;

            #region Properties
            PropertyInfo[] properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo pr in properties)
            {
                LinkedToAttribute la = pr.GetCustomAttribute<LinkedToAttribute>();
                if (la != null)
                {
                    var t = pr.PropertyType.IsGenericType ? pr.PropertyType.GetGenericArguments()[0] : la.As;

                    if (t != null)
                    {
                        object v;
                        if (la.Grp > -1 && la.Offs > -1)
                        {
                            MethodInfo mi = this.GetType().GetMethod("Var", new[] { typeof(long), typeof(long), typeof(int) })?.MakeGenericMethod(t);
                            v = mi?.Invoke(this, new object[] { la.Grp, la.Offs, la.Port });
                        }
                        else
                        {
                            MethodInfo mi = this.GetType().GetMethod("Var", new[] { typeof(string), typeof(int) })?.MakeGenericMethod(t);
                            v = mi?.Invoke(this, new object[] { la.To, la.Port });
                        }

                        if (v != null)
                            pr.SetValue(o, v);
                    } // if (t != null)
                } // if (la != null)
            } //foreach(PropertyInfo pr in properties)
            #endregion

            #region Fields
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo fi in fields)
            {
                LinkedToAttribute la = fi.GetCustomAttribute<LinkedToAttribute>();
                if (la != null)
                {
                    var t = fi.FieldType.IsGenericType ? fi.FieldType.GetGenericArguments()[0] : la.As;

                    if (t != null)
                    {
                        object v;
                        if (la.Grp > -1 && la.Offs > -1)
                        {
                            MethodInfo mi = this.GetType().GetMethod("Var", new[] { typeof(long), typeof(long), typeof(int) })?.MakeGenericMethod(t);
                            v = mi?.Invoke(this, new object[] { la.Grp, la.Offs, la.Port });
                        }
                        else
                        {
                            MethodInfo mi = this.GetType().GetMethod("Var", new[] { typeof(string), typeof(int) })?.MakeGenericMethod(t);
                            v = mi?.Invoke(this, new object[] { la.To, la.Port });
                        }

                        if (v != null)
                            fi.SetValue(o, v);
                    }
                } //if (la != null)
            } // foreach (FieldInfo fi in fields)
            #endregion

            return o;
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Var v in _dictNameVar.Values)
                    v.TryUnsubscribe();

                _cancelTokenSource?.Cancel();
            }
        }
    } // class
}