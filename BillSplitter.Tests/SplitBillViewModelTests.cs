using BillSplitter.ViewModels;
using NSubstitute;
using Shouldly;

namespace BillSplitter.Tests;

public class SplitBillViewModelTests
{
    private IStorageService storageService;
    private SplitBillViewModel vm;

    public SplitBillViewModelTests()
    {
        storageService = Substitute.For<IStorageService>();
        vm = new SplitBillViewModel(storageService);
    }

    [Fact]
    public void MoneyLeftOverOnePerson()
    {
        vm.TotalBillAmount = 100;
        addPerson("P1", 25);

        vm.RemainingAmount.ShouldBe(75);
    }

    [Fact]
    public void MoneyLeftOverMultiplePeople()
    {
        vm.TotalBillAmount = 100;
        addPerson("P1", 25);
        addPerson("P2", 25);
        addPerson("P3", 25);

        vm.RemainingAmount.ShouldBe(25);
    }

    [Fact]
    public void MoneyLeftOverGoesNegative()
    {
        vm.TotalBillAmount = 100;
        addPerson("P1", 25);
        addPerson("P2", 25);
        addPerson("P3", 75);

        vm.RemainingAmount.ShouldBe(-25);
    }

    [Fact]
    public void NotifyTipAmountChangedWhenBillAmountChanges()
    {
        int tipAmountChangedCount = 0;
        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(vm.TipAmount))
            {
                tipAmountChangedCount++;
            }
        };

        vm.TotalBillAmount = 100;
        vm.TotalBillAmount = 120;

        tipAmountChangedCount.ShouldBe(2);
    }

    [Fact]
    public void NotifyTipAmountChangedWhenTipPercentChanges()
    {
        int tipPercentChangedCount = 0;
        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(vm.TipAmount))
            {
                tipPercentChangedCount++;
            }
        };

        vm.TipPercent = .1M;
        vm.TipPercent = .15M;

        tipPercentChangedCount.ShouldBe(2);
    }

    [Fact]
    public void TipAmountAutomaticallyUpdates()
    {
        vm.TotalBillAmount = 100;
        vm.TipPercent = .15M;
        vm.TipAmount.ShouldBe(15);

        vm.TipPercent = .2M;
        vm.TipAmount.ShouldBe(20);
    }

    [Fact]
    public void ChangingAPersonsAmountRaisesPropertyChanged()
    {
        //Given an initial bill amount
        int amountChangedCount = 0;
        int myPartOfTipChangedCount = 0;
        int myTotalPortionChangedCount = 0;
        int remainingAmountChangedCount = 0;
        vm.TotalBillAmount = 100;

        //When I add a person
        vm.AddPersonCommand.Execute(this);

        //And I listen for property changed events on that new person
        vm.People[0].PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Person.Amount))
            {
                amountChangedCount++;
            }
            else if (e.PropertyName == nameof(Person.MyPartOfTip))
            {
                myPartOfTipChangedCount++;
            }
            else if (e.PropertyName == nameof(Person.MyTotalPortion))
            {
                myTotalPortionChangedCount++;
            }
        };
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.RemainingAmount))
            {
                remainingAmountChangedCount++;
            }
        };

        //When I change the person's amount twice
        vm.People[0].Amount = 50;
        vm.People[0].Amount = 75;

        //Then there should have been PropertyChanged events raised twice for each of these
        amountChangedCount.ShouldBe(2);
        myPartOfTipChangedCount.ShouldBe(2);
        myTotalPortionChangedCount.ShouldBe(2);
        remainingAmountChangedCount.ShouldBe(2);

        //And the remaining amount should be correct
        vm.RemainingAmount.ShouldBe(25);
    }

    [Fact]
    public void CannotSaveBillIfMissingPersonName()
    {
        vm.AddPersonCommand.Execute(this);
        vm.SaveBillCommand.CanExecute(this).ShouldBeFalse("Every person in the list needs a name");
    }

    [Fact]
    public void CannotSaveBillWithPersonNameButWith0Amount()
    {
        addPerson("Fred");
        vm.SaveBillCommand.CanExecute(this).ShouldBeFalse("Every person must have a positive amount");
    }

    [Fact]
    public void CannotSaveBillWithPersonNameAndAmountIfRemainingAmountIsNotZero()
    {
        vm.TotalBillAmount = 10;
        addPerson("Fred", 5);
        vm.SaveBillCommand.CanExecute(this).ShouldBeFalse("There are still $5 that someone owes");
    }

    [Fact]
    public void CanSaveBillIfRemainingAmountIsZeroAndAllPeopleHaveNamesAndPositiveValues()
    {
        int canExecuteChangeCount = 0;
        vm.SaveBillCommand.CanExecuteChanged += (_, _) => canExecuteChangeCount++;
        vm.TotalBillAmount = 10;
        addPerson("Fred", 10);
        canExecuteChangeCount.ShouldBeGreaterThan(0);
        vm.SaveBillCommand.CanExecute(this).ShouldBeTrue();
    }

    [Fact]
    public void ClickingSaveBillButtonSavesBill()
    {
        vm.TotalBillAmount = 10;
        addPerson("Fred", 10);
        vm.SaveBillCommand.Execute(this);
        storageService.Received().SaveBillAsync(vm);
    }

    private void addPerson(string? name = null, int? amount = null)
    {
        vm.AddPersonCommand.Execute(this);

        if (name is not null)
        {
            vm.People.Last().Name = name;
        }

        if (amount is not null)
        {
            vm.People.Last().Amount = amount.Value;
        }
    }
}
