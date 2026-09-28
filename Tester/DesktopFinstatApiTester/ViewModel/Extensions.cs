using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DesktopFinstatApiTester.ViewModel
{
    interface IModelViewModel<T>
    {
        void FromModel(T model);
        T ToModel(T model);
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the change notification on the UI thread. API calls run on the thread pool and write
        /// straight back into these view-models (limits, response headers), and Avalonia bindings —
        /// unlike WPF's — do not marshal a cross-thread notification for you. Doing it here fixes
        /// every producer at once instead of dispatching at each call site.
        /// </summary>
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null)
            {
                return;
            }
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(
                    () => handler(this, new PropertyChangedEventArgs(propertyName)));
            }
        }
    }
}
