using Microsoft.Maui.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using VirtualGridKit.Extension;

namespace VirtualGridKit
{
    public class DataAdapter : IDisposable
    {
        protected VirtualGrid Control { get; set; }
        protected List<object> InternalItems { get; set; } = [];
        protected List<object> InternalColumnHeaderItems { get; set; } = [];

        public IReadOnlyList<object> Items => InternalItems;

        public virtual int ItemsCount => InternalItems?.Count ?? 0;

        public event EventHandler DataSetChanged;
        public event EventHandler<(int startingIndex, int totalCount)> ItemRangeInserted;
        public event EventHandler<(int startingIndex, int totalCount)> ItemRangeRemoved;
        public event EventHandler<(int startingIndex, int oldCount, int newCount)> ItemRangeChanged;
        public event EventHandler<(int oldIndex, int newIndex)> ItemMoved;


        protected ScrollView _headerScrollView;
        protected ScrollView _contentScrollView;
        protected Grid _headerGrid;
        protected Grid _contentGrid;
        public DataAdapter(VirtualGrid listView)
        {
            Control = listView;
            Control.PropertyChanged += Control_PropertyChanged;
        }

        internal void InitializeComponent()
        {
            Control.RowDefinitions = new RowDefinitionCollection()
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            };

            _headerGrid = new Grid();
            _contentGrid = new Grid();
            _headerScrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                Content = _headerGrid,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never
            };
            _contentScrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Content = _contentGrid
            };

            Control.Add(_headerScrollView, 0, 0);
            Control.Add(_contentScrollView, 0, 1);
        }

        private void Control_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        public virtual void InitCollection(IEnumerable? itemsSource)
        {
            if (itemsSource is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += ItemsSourceCollectionChanged;
            }

            OnCollectionChangedReset(itemsSource);
        }

        public void BuildColumns()
        {
            if (Control.HeaderItemSource is null || Control.HeaderItemSource.Count() == 0)
            {
                return;
            }

            InternalColumnHeaderItems = [.. Control.HeaderItemSource];

            _headerGrid.ColumnDefinitions.Clear();
            foreach (var column in InternalColumnHeaderItems)
            {
                var columnDefinition = new ColumnDefinition()
                {
                    Width = Control.ColumnHeader.Width,
                };

                _headerGrid.ColumnDefinitions.Add(columnDefinition);
                _contentGrid.ColumnDefinitions.Add(columnDefinition);
            }
        }

        public void ReloadData()
        {
            var itemsSource = Control?.GridItemSource;

            RemoveListenerCollection(itemsSource);

            BuildColumns();

            InitCollection(itemsSource);
        }

        protected virtual void OnCollectionChangedReset(IEnumerable? itemsSource)
        {
            List<object> items = itemsSource is null ? [] : new(itemsSource.Cast<object>());

            InternalItems = items;

            NotifyDataSetChanged();
        }
        public virtual void NotifyDataSetChanged()
        {
            NotifyWrapper(() =>
            {
                DataSetChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private void NotifyWrapper(Action notifyAction)
        {
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(Notify);
            }
            else Notify();

            void Notify()
            {
                if (IsDisposed)
                {
                    RemoveListenerCollection(Control.GridItemSource);
                }
                else
                {
                    notifyAction.Invoke();
                }
            }
        }
        protected virtual void RemoveListenerCollection(IEnumerable? itemsSource)
        {
            if (itemsSource is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= ItemsSourceCollectionChanged;
            }
        }

        private void ItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var itemsSource = sender as IEnumerable;

            if (IsDisposed)
            {
                RemoveListenerCollection(itemsSource);
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnCollectionChangedAdd(e);
                    break;
                //case NotifyCollectionChangedAction.Remove:
                //    OnCollectionChangedRemove(e);
                //    break;
                //case NotifyCollectionChangedAction.Replace:
                //    OnCollectionChangedReplace(e);
                //    break;
                //case NotifyCollectionChangedAction.Move:
                //    OnCollectionChangedMove(e);
                //    break;
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChangedReset(itemsSource);
                    break;
            }
        }

        protected virtual void OnCollectionChangedAdd(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count is null or 0) return;

            var index = e.NewStartingIndex;

            InternalItems.InsertRange(index, e.NewItems.Cast<object>());
            NotifyItemRangeInserted(index, e.NewItems.Count);
        }

        public virtual void NotifyItemRangeInserted(int startingIndex, int totalCount)
        {
            NotifyWrapper(() =>
            {
                ItemRangeInserted?.Invoke(this, (startingIndex, totalCount));
            });
        }

        #region IDisposable
        public bool IsDisposed { get; protected set; }
        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed) return;

            Control.PropertyChanged -= Control_PropertyChanged;

            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DataAdapter()
        {
            Dispose(false);
        }
        #endregion

        public virtual List<DataTemplate> GetTemplate(int position)
        {

            return GetItemTemplate(InternalItems[position]);
        }

        protected virtual List<DataTemplate> GetItemTemplate(object item)
        {
            return Control.Columns.Select(x => x.ColumnCellTemplate).ToList();
        }
        public virtual double GetColumnsItemTemplate()
        {
            return Control.Columns.Sum(x => x.Width.Value);
        }

        public virtual CellHolder OnCreateCell(DataTemplate template, int position)
        {
            var holder = CreateEmptyCellForTemplate(template);
            var content = holder[0];

            return holder;
        }

        protected virtual CellHolder CreateEmptyCellForTemplate(DataTemplate template)
        {
            var content = template.CreateContent() as Microsoft.Maui.Controls.View;
            var holder = new CellHolder()
        {
            /*Content =*/ content
        };
            return holder;
        }

        public virtual void OnBindCell(ObservableCollection<CellHolder> holders, int position)
        {
            var data = Items[position];

            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].BindingContext = data;
            }
        }

        internal void OnAdapterSet()
        {
            Control.Adapter.DataSetChanged += AdapterDataSetChanged;
            Control.Adapter._headerScrollView.Scrolled += OnContentHeaderScrolled;
            Control.Adapter._contentScrollView.Scrolled += OnContentScrolled;
            //Control.Adapter.ItemMoved += AdapterItemMoved;
            //Control.Adapter.ItemRangeChanged += AdapterItemRangeChanged;
            //Control.Adapter.ItemRangeInserted += AdapterItemRangeInserted;
            //Control.Adapter.ItemRangeRemoved += AdapterItemRangeRemoved;
        }

        internal void UnsubscribeFromEvents()
        {
            Control.Adapter.DataSetChanged += AdapterDataSetChanged;
            Control.Adapter._headerScrollView.Scrolled += OnContentHeaderScrolled;
            Control.Adapter._contentScrollView.Scrolled += OnContentScrolled;
        }

        private void OnContentHeaderScrolled(object? sender, ScrolledEventArgs e)
        {
            //if ((e.ScrollX > 0 && _contentScrollView.ScrollX != e.ScrollX) || (e.ScrollY > 0 && _contentScrollView.ScrollY != e.ScrollY))
            //   _contentScrollView.ScrollToAsync(e.ScrollX, e.ScrollY, false);
            if ((e.ScrollX > 0 && _contentScrollView.ScrollX != e.ScrollX) || (e.ScrollY > 0 && _contentScrollView.ScrollY != e.ScrollY))
                _contentScrollView.ScrollToAsync(e.ScrollX, _contentScrollView.ScrollY, false);
        }

        private void OnContentScrolled(object? sender, ScrolledEventArgs e)
        {
            //if (e.ScrollX > 0 && e.ScrollY == 0) 
            //   _headerScrollView.ScrollToAsync(e.ScrollX, _headerScrollView.ScrollY, false);
            if (e.ScrollX > 0 && e.ScrollY == 0)
                _headerScrollView.ScrollToAsync(e.ScrollX, 0, false);
        }

        private void AdapterDataSetChanged(object? sender, EventArgs e)
        {

            if (InternalColumnHeaderItems.Count > 0)
            {
                InvalidateColumns();
            }

            if (InternalItems.Count > 0)
            {
                InvalidateLayout();
            }
        }

        private void InvalidateColumns()
        {
            for (int i = 0; i < InternalColumnHeaderItems.Count; i++)
            {
                var item = InternalColumnHeaderItems[i];

                var view = Control.ColumnHeader.HeaderTemplate.CreateContent() as View;
                view.BindingContext = item;
                _headerGrid.Add(view, i, 0);
            }
        }

        public void InvalidateLayout()
        {
            _contentGrid.RowDefinitions.Clear();

            for (int j = 0; j < InternalItems.Count; j++)
            {
                _contentGrid.AddRowDefinition(new RowDefinition()
                {
                    Height = GridLength.Auto,
                });
                var item = InternalItems[j];
                for (int i = 0; i < Control.Columns.Count; i++)
                {
                    var view = Control.Columns[i].ColumnCellTemplate.CreateContent() as View;
                    view.BindingContext = item;

                    _contentGrid.Add(view, i, j);
                }
            }
        }
    }
}
