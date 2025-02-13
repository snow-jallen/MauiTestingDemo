using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BillSplitter.ViewModels;

public partial class SplitBillViewModel : ObservableObject
{
    private readonly IStorageService storageService;

    public SplitBillViewModel(IStorageService storageService)
    {
        PropertyChanged += propertyChanged;
        this.storageService = storageService;
    }

    [ObservableProperty]
    private ObservableCollection<Person> people = [];

    [RelayCommand]
    private void AddPerson()
    {
        var p = new Person(this);
        p.PropertyChanged += propertyChanged;
        people.Add(p);
    }

    private void propertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //Note: There is an UpdateAmounts() method in the Person class...
        //      ...that may potentially prove useful. Maybe. It could anyway.
    }

    [ObservableProperty]
    private decimal totalBillAmount = 1;

    [ObservableProperty]
    private decimal tipPercent = .15M;

    public decimal RemainingAmount => 0M;

    public decimal TipAmount => 0M;

    [RelayCommand]
    private async Task SaveBill()
    {
    }
}

public partial class Person(SplitBillViewModel vm) : ObservableObject
{

    [ObservableProperty]
    private string name;

    private decimal amount;

    public decimal Amount
    {
        get => amount;
        set => SetProperty(ref amount, value);
    }

    public decimal MyPartOfTip => vm.TipAmount * (Amount / Math.Max(1, vm.TotalBillAmount));

    public decimal MyTotalPortion => Amount + MyPartOfTip;

    public void UpdateAmounts()
    {
        OnPropertyChanged(nameof(MyPartOfTip));
        OnPropertyChanged(nameof(MyTotalPortion));
    }
}

