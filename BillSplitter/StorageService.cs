using BillSplitter.ViewModels;
using System.Text.Json;

namespace BillSplitter;

public interface IStorageService
{
    List<PastBill> PastBills { get; }

    Task SaveBillAsync(SplitBillViewModel vm);
    event Action? OnDataChanged;
}

public class StorageService : IStorageService
{
    public StorageService()
    {
        string json = Preferences.Get("pastBills", string.Empty);
        if (json != string.Empty)
        {
            try
            {
                PastBills = JsonSerializer.Deserialize<List<PastBill>>(json);
            }
            catch (Exception ex)
            {
                PastBills = new();
            }
        }
        else
        {
            PastBills = new();
        }
    }
    public List<PastBill> PastBills { get; }
    public async Task SaveBillAsync(SplitBillViewModel vm)
    {
        PastBills.Add(new PastBill(vm.TotalBillAmount, "some summary"));
        Preferences.Set("pastBills", JsonSerializer.Serialize(PastBills));
        await Shell.Current.GoToAsync("//MainPage");
        OnDataChanged?.Invoke();
    }

    public event Action? OnDataChanged;
}

public record PastBill(decimal BillAmount, string summary);
