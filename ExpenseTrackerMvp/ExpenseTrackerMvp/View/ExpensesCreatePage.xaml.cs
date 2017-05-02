﻿
using ExpenseTrackerMvp.Service;
using ExpenseTrackerMvp.ViewModel;
using System;
using Xamarin.Forms;

namespace ExpenseTrackerMvp.View
{
    public partial class ExpensesCreatePage : ContentPage
    {
        public ExpensesCreatePage()
        {
            InitializeComponent();

            BindingContext = new ExpenseCreateViewModel(new ExpenseTrackerWebApiClientService());
            
            this.EntryDate.Date = DateTime.Now.Date;
            this.EntryValue.Text = string.Empty;
        }

        protected override void OnAppearing()
        {
            ((ExpenseCreateViewModel)BindingContext).LoadCategoriesCommand.Execute(null);
            ((ExpenseCreateViewModel)BindingContext).LoadPaymentTypesCommand.Execute(null);

            base.OnAppearing();
        }
                
    }
}
