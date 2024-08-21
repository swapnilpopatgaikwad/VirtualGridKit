using CommunityToolkit.Mvvm.ComponentModel;
using Sample.Model;
using System.Collections.ObjectModel;
using VirtualGridKit.Extension;

namespace Sample.ViewModel
{
    public partial class VirtualGridViewModel : ObservableObject
    {
        [ObservableProperty]
        public ObservableRangeCollection<VirtualGridModel> dataItems = [];
        [ObservableProperty]
        public ObservableRangeCollection<VirtualGridHeaderModel> headerItems = [];
        //[ObservableProperty]
        //public ObservableRangeCollection<DataGridColumn> columns = [];
        private static Random random = new Random();
        public VirtualGridViewModel()
        {
            BuildColumn();
            BuildDataItems();
        }

        private void BuildColumn()
        {
            var list = new List<VirtualGridHeaderModel>();
            for (int i = 0; i < 5; i++)
            {
                list.Add(new VirtualGridHeaderModel
                {
                    Title = $"Header {i}",
                });
            }

            HeaderItems.AddRange(list);
        }

        public void BuildDataItems()
        {
            var list = new List<VirtualGridModel>();
            for (int i = 0; i < 50; i++)
            {
                list.Add(new VirtualGridModel
                {
                    Index = i,
                    Name = $"Item Value {i}",
                    Name1 = "Name1",
                    Name2 = "Name2",
                    Name3 = "Name3",
                    Name4 = "Name4",
                    Name5 = "Name5",
                    Name6 = "Name6",
                    Name7 = "Name7",
                });
            }

            DataItems.AddRange(list);
        }
    }
}
