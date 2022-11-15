using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Leosac.KeyManager.Library.UI.Domain
{
    /// <summary>
    /// From https://www.codeproject.com/Articles/25808/Aggregating-WPF-Commands-with-CommandGroup
    /// </summary>
    public class CommandGroup : FreezableCollection<CommandReference>, ICommand
    {
        public CommandGroup() : base()
        {
            var me_colchange = this as INotifyCollectionChanged;
            me_colchange.CollectionChanged += OnCommandsCollectionChanged;
        }

        private IList<CommandReference> Commands
        {
            get
            {
                return this as IList<CommandReference>;
            }
        }

        /// <summary>
        /// Returns the collection of child commands. They are executed
        /// in the order that they exist in this collection.
        /// </summary>

        void OnCommandsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // We have a new child command so our ability to execute may have changed.
            this.OnCanExecuteChanged();

            if (e.NewItems != null && 0 < e.NewItems.Count)
            {
                foreach (ICommand cmd in e.NewItems)
                    cmd.CanExecuteChanged += this.OnChildCommandCanExecuteChanged;
            }

            if (e.OldItems != null && 0 < e.OldItems.Count)
            {
                foreach (ICommand cmd in e.OldItems)
                    cmd.CanExecuteChanged -= this.OnChildCommandCanExecuteChanged;
            }
            this.OnCanExecuteChanged();
        }

        void OnChildCommandCanExecuteChanged(object? sender, EventArgs e)
        {
            // Bubble up the child commands CanExecuteChanged event so that
            // it will be observed by WPF.
            this.OnCanExecuteChanged();
        }
        public bool CanExecute(object? parameter)
        {
            foreach (CommandReference cmd in this.Commands)
                if (!cmd.CanExecute(parameter))
                    return false;

            return true;
        }

        public event EventHandler? CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
                this.CanExecuteChanged(this, EventArgs.Empty);
        }

        public void Execute(object? parameter)
        {
            foreach (ICommand cmd in this.Commands)
                cmd.Execute(parameter);
        }
    }
}
