using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinstatApi;

namespace DesktopFinstatApiTester.ViewModel
{
    public class BasicResponse : ViewModel
    {
        public const string RequestHeadersProperty = "RequestHeaders";
        public const string ResponseHeadersProperty = "ResponseHeaders";

        private Dictionary<string, string[]> _requestHeaders = null;
        public Dictionary<string, string[]> RequestHeaders
        {
            get { return _requestHeaders; }
            set
            {
                if (_requestHeaders != value)
                {
                    _requestHeaders = value;
                    RaisePropertyChanged(RequestHeadersProperty);
                }
            }
        }

        private Dictionary<string, string[]> _responseHeaders = null;
        public Dictionary<string, string[]> ResponseHeaders
        {
            get { return _responseHeaders; }
            set
            {
                if (_responseHeaders != value)
                {
                    _responseHeaders = value;
                    RaisePropertyChanged(ResponseHeadersProperty);
                }
            }
        }
    }
    public class ResponseItem : BasicResponse
    {
        public const string RequestProperty = "Request";
        public const string ParameterProperty = "Parameter";
        public const string ParametersProperty = "Parameters";
        public const string ApiSourceProperty = "ApiSource";
        public const string ProfileProperty = "Profile";
        public const string SendProperty = "Send";
        public const string ReceivedProperty = "Received";
        public const string ContentProperty = "Content";
        public const string DataProperty = "Data";
        public const string DataCountProperty = "DataCount";

        public const string ResponseItemObjectProperty = "ResponseItem";

        public ResponseItem()
        {
            PropertyChanged += ResponseItem_PropertyChanged;
            Send = DateTime.UtcNow;
            Data = new object[0];
        }

        public ResponseItem(string request, string apisource, IEnumerable<object> parameters) : this()
        {
            Request = request;
            Parameters = parameters;
            ApiSource = apisource;
        }

        private DateTime _send = DateTime.UtcNow;
        public DateTime Send
        {
            get { return _send; }
            set
            {
                if (_send != value)
                {
                    _send = value;
                    RaisePropertyChanged(SendProperty);
                }
            }
        }

        private string _request = null;
        public string Request
        {
            get { return _request; }
            set
            {
                if (_request != value)
                {
                    _request = value;
                    RaisePropertyChanged(RequestProperty);
                }
            }
        }

        /// <summary>The comparison this row's call was made by; the grid lists it under that comparison's row.</summary>
        public string GroupId { get; set; }

        /// <summary>The Request column: a comparison's call is marked as sitting under the comparison's row.</summary>
        public string DisplayRequest => GroupId != null ? "↳ " + Request : Request;

        /// <summary>Set on a comparison's own row: opening it brings the comparison back as it was left.</summary>
        public CompareSession Compare { get; set; }

        private string _profile;
        /// <summary>Environment profile the call went to (name, plus the port when one was set).</summary>
        public string Profile
        {
            get { return _profile; }
            set
            {
                if (_profile != value)
                {
                    _profile = value;
                    RaisePropertyChanged(ProfileProperty);
                }
            }
        }

        private string _apiSource = null;
        public string ApiSource
        {
            get { return _apiSource; }
            set
            {
                if (_apiSource != value)
                {
                    _apiSource = value;
                    RaisePropertyChanged(ApiSourceProperty);
                }
            }
        }

        private IEnumerable<object> _parameters = null;
        public IEnumerable<object> Parameters
        {
            get { return _parameters; }
            set
            {
                if (_parameters != value)
                {
                    _parameters = value;
                    RaisePropertyChanged(ParametersProperty);
                    RaisePropertyChanged(ParameterProperty);
                }
            }
        }

        public string Parameter
        {
            get
            {
                return string.Join("; ", Parameters?.Select(x => $"{x}") ?? Array.Empty<string>());
            }
        }

        private DateTime? _received = null;
        public DateTime? Received
        {
            get { return _received; }
            set
            {
                if (_received != value)
                {
                    _received = value;
                    RaisePropertyChanged(ReceivedProperty);
                }
            }
        }

        private object[] _data = null;
        public object[] Data
        {
            get { return _data; }
            set
            {
                if (_data != value)
                {
                    _data = value;
                    RaisePropertyChanged(DataProperty);
                    RaisePropertyChanged(DataCountProperty);
                }
            }
        }

        public int DataCount
        {
            get
            {
                return (Data != null) ? Data.Length : 0;
            }
        }

        private byte[] _content = null;
        public byte[] Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    RaisePropertyChanged(ContentProperty);
                }
            }
        }

        private void ResponseItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (new[] {RequestProperty, ParameterProperty, SendProperty, ReceivedProperty, DataProperty, ApiSourceProperty, ResponseHeadersProperty, RequestHeadersProperty }.Contains(e.PropertyName))
            {
                RaisePropertyChanged(ResponseItemObjectProperty);
            }
        }

        internal void AddData(object[] data)
        {
            Data = data;
            Received = DateTime.UtcNow;
        }
    }
}
