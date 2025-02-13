using BillSplitter.ViewModels;

namespace BillSplitter;

public partial class SplitBill : ContentPage
{
    public SplitBill(SplitBillViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}