using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

// code from https://stackoverflow.com/questions/3668802/looking-for-an-object-graph-tree-view-control-for-wpf?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
namespace DesktopFinstatApiTester.ViewModel
{
    public class ObjectViewModel : INotifyPropertyChanged
    {
        ReadOnlyCollection<ObjectViewModel> _children;
        readonly ObjectViewModel _parent;
        readonly object _object;
        readonly PropertyInfo _info;
        readonly Type _type;

        bool _isExpanded;
        bool _isSelected;

        public ObjectViewModel(object obj)
            : this(obj, null, null)
        {
        }

        ObjectViewModel(object obj, PropertyInfo info, ObjectViewModel parent)
        {
            _object = obj;
            _info = info;
            if (_object != null)
            {
                _type = obj.GetType();
                if (!IsPrintableType(_type))
                {
                    // load the _children object with an empty collection to allow the + expander to be shown
                    _children = new ReadOnlyCollection<ObjectViewModel>(new ObjectViewModel[] { new ObjectViewModel(null) });
                }
            }
            _parent = parent;
        }

        public void LoadChildren(bool triggerPropertyChanged)
        {
            if (_object != null && (_children == null || _children.Count == 0 || (_children.Count == 1 && _children[0]._object == null)))
            {
                // exclude value types and strings from listing child members
                if (!IsPrintableType(_type))
                {
                    var type = _object.GetType();
                    var dictionary = _object as System.Collections.IDictionary;
                    // the public properties of this object are its children
                    // if this is a collection type, add the contained items to the children

                    var children = (dictionary == null && !type.IsArray)
                        ? _type.GetProperties() .Where(p => !p.GetIndexParameters().Any()) // exclude indexed parameters for now
                        .Select(p => new ObjectViewModel(p.GetValue(_object, null), p, this))
                        .ToList()
                    : new List<ObjectViewModel>();
                    if (type.IsArray)
                    {
                        foreach (var item in (Array)_object)
                        {
                            children.Add(new ObjectViewModel(item, null, this)); // todo: add something to view the index value
                        }
                    }
                    else if (dictionary != null)
                    {
                        foreach (var item in dictionary)
                        {
                            children.Add(new ObjectViewModel(item, null, this)); // todo: add something to view the index value
                        }
                    }

                    _children = new ReadOnlyCollection<ObjectViewModel>(children);
                    foreach (var child in _children)
                    {
                        child.LoadChildren(false);
                    }
                    if (triggerPropertyChanged)
                    {
                        this.OnPropertyChanged("Children");
                        this.OnPropertyChanged("PropertyInfo");
                        this.OnPropertyChanged("PropertyDetail");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the object graph can display this type without enumerating its children
        /// </summary>
        static bool IsPrintableType(Type type)
        {
            return type != null && (
                type.IsPrimitive ||
                type.IsAssignableFrom(typeof(string)) ||
                type.IsAssignableFrom(typeof(decimal)) ||
                type.IsAssignableFrom(typeof(DateTime)) ||
                type.IsAssignableFrom(typeof(DateTime?)) ||
                type.IsEnum);
        }

        public object Object
        {
            get { return _object; }
        }

        public ObjectViewModel Parent
        {
            get { return _parent; }
        }

        public PropertyInfo Info
        {
            get { return _info; }
        }

        public ReadOnlyCollection<ObjectViewModel> Children
        {
            get { return _children; }
        }

        public string Type
        {
            get
            {
                System.Type type = null;
                if (_object != null)
                {
                    type = _type;
                }
                else
                {
                    if (_info != null)
                    {
                        type = _info.PropertyType;
                    }
                }
                return string.Format("({0})", type?.Name?.Replace("[]", $"[{_children?.Count}]"));
            }
        }

        public string Name
        {
            get
            {
                var name = string.Empty;
                if (_info != null)
                {
                    name = _info.Name;
                }
                return name;
            }
        }

        public string Value
        {
            get
            {
                var value = string.Empty;
                if (_object != null)
                {
                    if (IsPrintableType(_type))
                    {
                        value = _object.ToString();
                    }
                }
                else
                {
                    value = "<null>";
                }
                return value;
            }
        }

        public string PropertyInfo
        {
            get
            {
                return string.Format("{0}[{1}]:",
                    Name,
                    Type
                );
            }
        }

        public string PropertyDetail
        {
            get
            {
                return string.Format("{0}",
                    Children != null && Children.Any() && Children.Any(x => x._object != null) ? string.Format(" Children: {0}", Children.Count(x => x._object != null)) : null
                );
            }
        }

        #region Presentation Members

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    if (_isExpanded)
                    {
                        LoadChildren(true);
                    }
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                {
                    _parent.IsExpanded = true;
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public bool NameContains(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(Name))
            {
                return false;
            }

            return Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public bool ValueContains(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(Value))
            {
                return false;
            }

            return Value.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public override string ToString()
        {
            return LevelToString(0);
        }

        protected virtual string LevelToString(int level)
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat("{0} {1}", PropertyInfo, Value);
            if (Children != null && Children.Any() && Children.Any(x => x._object != null))
            {
                int childlevel = level + 1;
                foreach (var child in Children)
                {
                    result.AppendLine();
                    result.AppendFormat("{0} {1}", string.Empty.PadLeft(childlevel, '\t'), child.LevelToString(childlevel));
                }
            }
            return result.ToString();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
