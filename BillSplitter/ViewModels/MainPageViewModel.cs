using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillSplitter.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public MainPageViewModel(IStorageService storageService)
    {
        this.storageService = storageService;
        storageService.OnDataChanged += StorageService_OnDataChanged;
    }

    private void StorageService_OnDataChanged()
    {
        OnPropertyChanged(nameof(PastBills));
    }

    [ObservableProperty]
    private string pageTitle = "Bill Super Splitter";
    private readonly IStorageService storageService;

    [RelayCommand]
    private void GoSplitBill()
    {
        Shell.Current.GoToAsync("//SplitBill");
    }

    public IEnumerable<PastBill> PastBills => storageService.PastBills;
}
