using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace Spring.Framework
{
	public class ObservableItemsCollection<T> : BaseViewModel, IEnumerable<T> , INotifyCollectionChanged where T : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler ItemPropertyChanged;
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		protected bool BroadcastCollectionChanges = true;
		protected bool BroadcastItemChanges = true;

		private readonly ObservableCollection<T> _sourceObservableCollection = new ObservableCollection<T>();
        public ObservableItemsCollection() {
			this._sourceObservableCollection.CollectionChanged += propertyChangeHandlerSubscriptions;
            this._sourceObservableCollection.CollectionChanged += BroadcastCollectionChange;
		}

        public ObservableItemsCollection(IEnumerable<T> items) {
			this._sourceObservableCollection.CollectionChanged += propertyChangeHandlerSubscriptions;
			items.ToList().ForEach(item => _sourceObservableCollection.Add(item));
			this._sourceObservableCollection.CollectionChanged += BroadcastCollectionChange;
		}

		private void BroadcastCollectionChange(object sender, NotifyCollectionChangedEventArgs e)
		{
            if (BroadcastCollectionChanges && this.CollectionChanged != null)
            {
				this.CollectionChanged(this, e);
            }
		}

		private void propertyChangeHandlerSubscriptions(object sender, NotifyCollectionChangedEventArgs e)
        {
			if(e.Action == NotifyCollectionChangedAction.Reset)
            {
				_sourceObservableCollection.ToList().ForEach(item => item.PropertyChanged += OnItemPropertyChanged);
            }
			e.NewItems?.Cast<T>().ToList().ForEach(item => item.PropertyChanged += OnItemPropertyChanged);
			e.OldItems?.Cast<T>().ToList().ForEach(item => item.PropertyChanged -= OnItemPropertyChanged);
        }

        private void ObservableItemsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {			
			if (e.Action == NotifyCollectionChangedAction.Reset)
            {
				this.ToList().ForEach(item => item.PropertyChanged += OnItemPropertyChanged);
            }
            else
            {
				e.NewItems?.Cast<T>().ToList().ForEach(item => item.PropertyChanged += OnItemPropertyChanged);
				e.OldItems?.Cast<T>().ToList().ForEach(item => item.PropertyChanged -= OnItemPropertyChanged);
			}
		}		

		public virtual void ResetRange(params T[] items)
        {
			ResetRange(items.ToList());
        }

        public virtual void ResetRange(IEnumerable<T> items = null)
		{
			BroadcastCollectionChanges = false;
			bool didChange = false;
			if(_sourceObservableCollection.Count > 0)
            {
				_sourceObservableCollection.ToList().ForEach(item => 
					_sourceObservableCollection.Remove(item));
				didChange = true;
			}
			if (items != null && items.Count() > 0)
			{
				items.ToList().ForEach(item => 
					_sourceObservableCollection.Add(item));
				didChange = true;
			}
			BroadcastCollectionChanges = true;
            if (didChange)
            {
				BroadcastCollectionChange(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		public void ReplaceItem(T current, T updated)
        {
			var i = _sourceObservableCollection.IndexOf(current);
            if (i > -1)
            {
				_sourceObservableCollection[i] = updated;
            }
        }

		public virtual void AddRange(params T[] items)
        {
			AddRange(items.ToList());
        }

		public virtual void AddRange(IEnumerable<T> items)
		{
			if(items != null && items.Count() > 0)
			{
				BroadcastCollectionChanges = false;
				items.ToList().ForEach(item => _sourceObservableCollection.Add(item));
				BroadcastCollectionChanges = true;
				var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList());
				BroadcastCollectionChange(this, args);
			}
		}

		public virtual void RemoveRange(params T[] items)
        {
			RemoveRange(items.ToList());
        }
		public virtual void RemoveRange(IEnumerable<T> items)
		{
			if(items != null && items.Count() > 0)
			{
				BroadcastCollectionChanges = false;
				bool didChange = false;
				items.ToList().ForEach(item => {
                    if (_sourceObservableCollection.Contains(item))
                    {
						_sourceObservableCollection.Remove(item);
						didChange = true;
                    }
				});
				BroadcastCollectionChanges = true;
				if (didChange)
                {
					BroadcastCollectionChange(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items));
                }
			}
		}

		protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (BroadcastItemChanges && this.ItemPropertyChanged != null)
			{
				this.ItemPropertyChanged(sender, e);
			}
		}

		public virtual int Count => _sourceObservableCollection.Count;

        public IEnumerator<T> GetEnumerator()
        {
			return _sourceObservableCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
			return GetEnumerator();
        }
    }
}
