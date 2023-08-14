using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Spring.Framework
{
    public interface IIsVisible
    {
        bool IsVisible { get; set; }
    }
    public class FilterableObservableItemsColleciton<T> : ObservableItemsCollection<T> where T : IIsVisible, INotifyPropertyChanged
    {
        public FilterableObservableItemsColleciton() : base() { }
        public FilterableObservableItemsColleciton(IEnumerable<T> items) : base(items) {
        }

        private void FilterableObservableItemsColleciton_ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (T)sender;
            if(e.PropertyName == nameof(item.IsVisible))
            {
                var range = new List<T> { item };
                if (item.IsVisible)
                {
                    this.AddRange(range);
                    hiddenItems.Remove(item);
                }
                else
                {
                    this.RemoveRange(new List<T> { item });
                    hiddenItems.Add(item);
                }
            }
        }

        private readonly List<T> hiddenItems = new List<T>();
    }
}
