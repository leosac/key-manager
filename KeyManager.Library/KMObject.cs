using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public abstract class KMObject : NotifyPropertyBase, INotifyDataErrorInfo
    {
        public KMObject()
        {
            Errors = new PropertyErrors(this, OnErrorsChanged);
        }

        public PropertyErrors Errors { get; private set; }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
            => this.ErrorsChanged?.Invoke(this, e);

        public bool HasErrors => Errors.HasErrors;

        public IEnumerable GetErrors(string? propertyName) => Errors.GetErrors(propertyName);
    }
}
