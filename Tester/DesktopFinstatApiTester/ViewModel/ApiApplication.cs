using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DesktopFinstatApiTester.ViewModel
{
    public class ApiApplication : ViewModel
    {
        private const string fileName = @"application.json";
        public const string SettingsProperty = "Settings";
        public const string LimitsProperty = "Limits";
        public const string ResponseItemsProperty = "ResponseItems";
        public const string ApplicationObjectProperty = "ApplicationObject";

        public ApiApplication()
        {
            PropertyChanged += ApiApplication_PropertyChanged;
            Settings = new Model.Settings();
            Load();
        }

        public Model.Settings Settings { get; set; }

        public Limits _limits = new Limits();
        public Limits Limits
        {
            get { return _limits; }
            set
            {
                if (_limits != null)
                {
                    _limits.PropertyChanged -= _limits_PropertyChanged;
                }
                _limits = value;
                if (_limits != null)
                {
                    _limits.PropertyChanged += _limits_PropertyChanged;
                }
            }
        }

        private ObservableCollection<ResponseItem> _items = new ObservableCollection<ResponseItem>();
        public ObservableCollection<ResponseItem> ResponseItems
        {
            get { return _items; }
            set { _items = value; RaisePropertyChanged(ResponseItemsProperty); }
        }

        private void _limits_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Limits.LimitsObjectProperty)
            {
                RaisePropertyChanged(LimitsProperty);
            }
        }

        internal ResponseItem Add(string request, string apisource, IEnumerable<object> parameters)
        {
            return Add(new ResponseItem(request, apisource, parameters));
        }

        internal ResponseItem Add(ResponseItem item)
        {
            if (ResponseItems != null)
            {
                ResponseItems.Insert(0, item);
                RaisePropertyChanged(ResponseItemsProperty);
                return item;
            }
            return null;
        }

        internal ResponseItem RemoveAt(int index)
        {
            if (index < ResponseItems.Count)
            {
                ResponseItem result = ResponseItems[index];
                ResponseItems.RemoveAt(index);
                return result;
            }
            return null;
        }

        public void Load()
        {
            if (File.Exists(fileName))
            {
                Settings = JsonSerializer.Deserialize<Model.Settings>(File.ReadAllText(fileName));
            }
        }

        public void Save()
        {
            File.WriteAllText(fileName, JsonSerializer.Serialize(Settings));
        }

        private void ApiApplication_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (new[] { SettingsProperty }.Contains(e.PropertyName))
            {
                RaisePropertyChanged(ApplicationObjectProperty);
            }
        }
    }
}
