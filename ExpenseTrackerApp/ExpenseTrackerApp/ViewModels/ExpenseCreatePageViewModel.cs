﻿using ExpenseTrackerApp.Model;
using ExpenseTrackerApp.Service;
using ExpenseTrackerApp.Settings;
using ExpenseTrackerApp.Views;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ExpenseTrackerApp.ViewModels
{
    public class ExpenseCreatePageViewModel : BaseViewModel, INavigatedAware
    {
        public DateTime Date { get; set; }
        public String Description { get; set; }
        public Double Value { get; set; }
        public String Category { get; set; }
        public String PaymentType { get; set; }

        public ObservableCollection<string> CategoryList { get; }

        private string _categorySelectedItem;
        public string CategorySelectedItem
        {
            get { return _categorySelectedItem; }
            set { SetProperty(ref _categorySelectedItem, value); }            
        }

        public ObservableCollection<string> PaymentTypeList { get; }

        private string _paymentTypeSelectedItem;
        public string PaymentTypeSelectedItem
        {
            get { return _paymentTypeSelectedItem; }
            set { SetProperty(ref _paymentTypeSelectedItem, value); }
        }




        public DelegateCommand SaveCommand => new DelegateCommand(async () => await ExecuteSaveAsync());
        
        public DelegateCommand BackCommand => new DelegateCommand(async () => await ExecuteBackAsync());


        
        private readonly IExpenseTrackerService _expenseTrackerService;
        private readonly INavigationService _navigationService;
        private readonly IUserSettings _userSettings;

        public ExpenseCreatePageViewModel(IExpenseTrackerService expenseTrackerService, 
            INavigationService navigationService,
            IUserSettings userSettings)
        {
            _expenseTrackerService = expenseTrackerService;
            _navigationService = navigationService;
            _userSettings = userSettings;

            PaymentTypeList = new ObservableCollection<string>();

            CategoryList = new ObservableCollection<string>();
        }




        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            this.Date = DateTime.Now.Date;
            this.Description = string.Empty;

            await ExecuteLoadCategoriesAsync();
            await ExecuteLoadPaymentTypesAsync();

        }


        public async Task ExecuteLoadCategoriesAsync()
        {

            try
            {
                IsBusy = true;

                CategoryList.Clear();

                List<Category> listCat = await _expenseTrackerService.GetCategoryListAsync();

                foreach (Category cat in listCat)
                {
                    CategoryList.Add(cat.Name);
                }

            }
            catch (Exception ex)
            {
                await base.ShowErrorMessage("Error getting category list : " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }

        }

        public async Task ExecuteLoadPaymentTypesAsync()
        {

            try
            {
                IsBusy = true;

                PaymentTypeList.Clear();

                List<PaymentType> listPaymentType = await _expenseTrackerService.GetPaymentTypeListAsync();

                foreach (PaymentType pt in listPaymentType)
                {
                    PaymentTypeList.Add(pt.Name);
                }

            }
            catch (Exception ex)
            {
                await base.ShowErrorMessage("Error getting payment type list : " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }

        }




        int _tapCount = 0;

        private async Task ExecuteSaveAsync()
        {
            // Prevent multiple tap on save button
            _tapCount += 1;
            if (_tapCount > 1)
            {
                _tapCount = 0;
                return;
            }

            Expense exp = new Expense();
            exp.Date = this.Date;

            if (this.CategorySelectedItem == null)
            {
                await base.ShowErrorMessage("Select a category.");
                return;
            }
            exp.Category = this.CategorySelectedItem;

            if (this.Value == 0)
            {
                await base.ShowErrorMessage("Inform a value.");
                return;
            }
            exp.Value = this.Value;

            if (this.PaymentTypeSelectedItem == null)
            {
                await base.ShowErrorMessage("Select a payment type.");
                return;
            }
            exp.PaymentType = this.PaymentTypeSelectedItem;


            if (string.IsNullOrEmpty(this.Description))
            {
                exp.Description = exp.Category;
            }
            else
            {
                exp.Description = this.Description;
            }


            exp.UserName = _userSettings.GetEmail();

            
            if (await _expenseTrackerService.SaveExpenseAsync(exp))
            {
                string imageToShow = GetImageToShow(exp);
                await NavigateBackShowingImageAsync(imageToShow);
            }
            else
            {
                await base.ShowErrorMessage("Error creating Expense on server.");
            }
        }

        private string GetImageToShow(Expense exp)
        {
            string img_s = "puppys{0}.png";
            string img_h = "puppyh{0}.png";

            Random rnd = new Random();


            if ((DateTime.Now.DayOfWeek.Equals(DayOfWeek.Monday) && exp.Value > 30) || exp.Value > 80)
            {
                int r = rnd.Next(1, 7);
                return string.Format(img_s, r);
            }
            else if ((DateTime.Now.DayOfWeek.Equals(DayOfWeek.Tuesday) && exp.Value > 40))
            {
                int r = rnd.Next(1, 7);
                return string.Format(img_s, r);
            }
            else
            {
                int r = rnd.Next(1, 8);
                return string.Format(img_h, r);
            }

        }

        private async Task NavigateBackShowingImageAsync(string imageToShow)
        {
            // Show image after save expense
            if (_userSettings.GetShowPuppyPref() && imageToShow != null)
            {
                NavigationParameters navParameters = new NavigationParameters();
                navParameters.Add("imageToShow", imageToShow);
                await _navigationService.NavigateAsync(nameof(ShowGifPage), navParameters, true, true);
                await Task.Delay(1500);
                await _navigationService.GoBackAsync();
            }

            await _navigationService.GoBackAsync();
        }


        private async Task ExecuteBackAsync()
        {
            await _navigationService.GoBackAsync();
        }
    }
}
