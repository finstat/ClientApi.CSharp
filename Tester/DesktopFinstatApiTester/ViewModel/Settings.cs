using DesktopFinstatApiTester.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DesktopFinstatApiTester.ViewModel
{
    public class Settings : ViewModel, IModelViewModel<Model.Settings>
    {
        public const string ApiKeysProperty = "ApiKeys";
        public const string ResponseTypeProperty = "ResponseType";
        public const string LangProperty = "Lang";
        public const string TimeOutProperty = "TimeOut";
        public const string StationIDProperty = "StationID";
        public const string StationNameProperty = "StationName";
        public const string FinStatApiUrlProperty = "FinStatApiUrl";
        public const string FinStatApiUrlPropertyCZ = "FinStatApiUrlCZ";
        public const string SettingsObjectProperty = "SettingsObject";

        public Settings()
        {
            PropertyChanged += Settings_PropertyChanged;
            ApiKeys = new ApiKeys();
            FromModel(new Model.Settings());
        }

        private ApiKeys _apiKeys;
        public ApiKeys ApiKeys
        {
            get { return _apiKeys; }
            set
            {
                if (_apiKeys != value)
                {
                    if(_apiKeys != null)
                    {
                        _apiKeys.PropertyChanged -= _apiKeys_PropertyChanged;
                    }
                    _apiKeys = value;
                    _apiKeys.PropertyChanged += _apiKeys_PropertyChanged;

                }
            }
        }

        /// <summary>
        /// Flat pass-throughs to <see cref="ApiKeys"/>. Birko's schema-driven <c>Form</c> binds each
        /// <c>FormField.Name</c> as a path on the model, so exposing the two keys at the top level
        /// keeps the settings schema a flat list of field names.
        /// </summary>
        public string PublicKey
        {
            get { return ApiKeys?.PublicKey; }
            set { if (ApiKeys != null) { ApiKeys.PublicKey = value; RaisePropertyChanged(nameof(PublicKey)); } }
        }

        public string PrivateKey
        {
            get { return ApiKeys?.PrivateKey; }
            set { if (ApiKeys != null) { ApiKeys.PrivateKey = value; RaisePropertyChanged(nameof(PrivateKey)); } }
        }

        private int _timeOut;
        public int TimeOut
        {
            get { return _timeOut; }
            set
            {
                if (_timeOut != value)
                {
                    _timeOut = value;
                    RaisePropertyChanged(TimeOutProperty);
                }
            }
        }

        private string _stationName;
        public string StationName
        {
            get { return _stationName; }
            set
            {
                if (_stationName != value)
                {
                    _stationName = value;
                    RaisePropertyChanged(StationNameProperty);
                }
            }
        }

        private string _stationID;
        public string StationID
        {
            get { return _stationID; }
            set
            {
                if (_stationID != value)
                {
                    _stationID = value;
                    RaisePropertyChanged(StationIDProperty);
                }
            }
        }

        private string _FinStatApiUrl;
        public string FinStatApiUrl
        {
            get { return _FinStatApiUrl; }
            set
            {
                if (_FinStatApiUrl != value)
                {
                    _FinStatApiUrl = value;
                    RaisePropertyChanged(FinStatApiUrlProperty);
                }
            }
        }

        private string _FinStatApiUrlCZ;
        public string FinStatApiUrlCZ
        {
            get { return _FinStatApiUrlCZ; }
            set
            {
                if (_FinStatApiUrlCZ != value)
                {
                    _FinStatApiUrlCZ = value;
                    RaisePropertyChanged(FinStatApiUrlPropertyCZ);
                }
            }
        }

        private ResponseType _responseType;
        public ResponseType ResponseType
        {
            get { return _responseType; }
            set
            {
                if (_responseType != value)
                {
                    _responseType = value;
                    RaisePropertyChanged(ResponseTypeProperty);
                }
            }
        }

        private ResponseLang _lang;
        public ResponseLang Lang
        {
            get { return _lang; }
            set
            {
                if (_lang != value)
                {
                    _lang = value;
                    RaisePropertyChanged(LangProperty);
                }
            }
        }

        private void _apiKeys_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(new[] { ApiKeys.ApiKeysObjectProperty }.Contains(e.PropertyName))
            {
                RaisePropertyChanged(ApiKeysProperty);
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (new[] { TimeOutProperty, StationNameProperty, StationIDProperty, ApiKeysProperty, ResponseTypeProperty, LangProperty, FinStatApiUrlProperty, FinStatApiUrlPropertyCZ }.Contains(e.PropertyName))
            {
                RaisePropertyChanged(SettingsObjectProperty);
            }
        }

        /// <summary>
        /// Per-field errors, keyed by field name; empty when the form is valid. Replaces the WPF
        /// <c>IAppControl.GetErrors()</c> walk over <c>Validation.GetErrors(textBox)</c> — no
        /// <c>ValidationRule</c>s were ever declared, so the checks were these emptiness tests.
        /// </summary>
        public IDictionary<string, string[]> Validate()
        {
            var errors = new Dictionary<string, string[]>();

            void Require(string field, string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add(field, new[] { "Value is empty" });
                }
            }

            Require(nameof(PublicKey), PublicKey);
            Require(StationNameProperty, StationName);
            Require(StationIDProperty, StationID);
            Require(FinStatApiUrlProperty, FinStatApiUrl);
            Require(FinStatApiUrlPropertyCZ, FinStatApiUrlCZ);
            if (TimeOut <= 0)
            {
                errors.Add(TimeOutProperty, new[] { "Value must be greater than zero" });
            }

            return errors;
        }

        public bool IsValid()
        {
            return Validate().Count == 0;
        }

        public void FromModel(Model.Settings model)
        {
            if (model != null)
            {
                ApiKeys.FromModel(model.ApiKeys);
                RaisePropertyChanged(nameof(PublicKey));
                RaisePropertyChanged(nameof(PrivateKey));
                ResponseType = model.ResponseType;
                Lang = model.Lang;
                TimeOut = model.TimeOut;
                StationName = model.StationName;
                StationID = model.StationID;
                FinStatApiUrl = model.FinStatApiUrl;
                FinStatApiUrlCZ = model.FinStatApiUrlCZ;
            }
        }

        public Model.Settings ToModel(Model.Settings model)
        {
            if (model == null)
            {
                model = new Model.Settings();
            }
            model.ApiKeys = ApiKeys.ToModel(model.ApiKeys);
            model.ResponseType = ResponseType;
            model.Lang = Lang;
            model.StationName = StationName;
            model.StationID = StationID;
            model.TimeOut = TimeOut;
            model.FinStatApiUrl = FinStatApiUrl;
            model.FinStatApiUrlCZ = FinStatApiUrlCZ;

            return model;
        }
    }
}
