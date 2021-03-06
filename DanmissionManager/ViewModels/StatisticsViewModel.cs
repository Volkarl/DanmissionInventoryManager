﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanmissionManager.Commands;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using DanmissionManager.Models;

namespace DanmissionManager.ViewModels
{
    class StatisticsViewModel : BaseViewModel
    {
        public StatisticsViewModel(Popups popupService) : base(popupService)
        {
            this.Statistics = new ObservableCollection<string>(StatCombobox());
            this.CommandDisplayChart = new RelayCommand(ChangeChart);
            
            TimeSpan timespan = new TimeSpan(30, 0, 0, 0);
            this.DateFrom = DateTime.Now - timespan;
            this.DateTo = DateTime.Now;

            //get all categories and soldproducts
            GetDatabaseData();

            this.SelectedChart = Statistics.First();
            ChangeChart();
        }

        #region Properties
        
        private ObservableCollection<string> _statistics;
        public ObservableCollection<string> Statistics
        {
            get { return _statistics; }
            set { _statistics = value; OnPropertyChanged("Statistics"); }
        }
        
        public List<SoldProduct> AllSoldProducts { get; set; }
        public List<Product> AllProducts { get; set; }

        private List<Category> _allCategories;
        public List<Category> AllCategories
        {
            get { return _allCategories; }
            set { _allCategories = value; OnPropertyChanged("Allcategories"); }
        }

        private List<KeyValuePair<string, int>> _pieChart;
        public List<KeyValuePair<string, int>> PieChart
        {
            get { return _pieChart; }
            set { _pieChart = value; OnPropertyChanged("PieChart"); }
        }

        private string _selectedChart;
        public string SelectedChart
        {
            get { return _selectedChart; }
            set { _selectedChart = value; OnPropertyChanged("SelectedChart"); }
        }

        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set { _dateFrom = value; OnPropertyChanged("DateFrom"); }
        }

        private DateTime _dateTo;
        public DateTime DateTo
        {
            get { return _dateTo; }
            set { _dateTo = value; OnPropertyChanged("DateTo"); }
        }
        
        #endregion
        
        #region CommandProperties

        public RelayCommand CommandDisplayChart { get; }

        #endregion

        #region Methods

        private void GetDatabaseData()
        {
            try
            {
                using (var ctx = new ServerContext())
                {
                    this.AllCategories = new List<Category>(ctx.Category.ToList());
                    this.AllSoldProducts = new List<SoldProduct>(ctx.Soldproducts.ToList());
                    this.AllProducts = new List<Product>(ctx.Products.ToList());
                }
            }
            catch (System.Data.DataException)
            {
                PopupService.PopupMessage(Application.Current.FindResource("CouldNotConnectToDatabase").ToString(), Application.Current.FindResource("Error").ToString());
            }
        }

        private void CalculateSum()
        {

            for (int i = 0; i < this.AllCategories.Count; i++)
            {
                List<SoldProduct> list = new List<SoldProduct>();
                list = AllSoldProducts.Where(x => x.category.CompareTo(AllCategories[i].id) == 0).ToList();

                AllSoldProducts.Where(x => x.category.CompareTo(AllCategories[i].id) == 0).Sum(x => (int) x.price);

                this.AllCategories[i].Sum = list.Sum(y => (int) y.price);
            }
        }
        
        private void ChangeChart()
        {
            if (this.SelectedChart == Application.Current.FindResource("SRevenuePerCategory").ToString())
            {
                ShowChartSales();
            }
            else if(this.SelectedChart == Application.Current.FindResource("SProductsSoldPerCategory").ToString())
            {
                ShowChartItems();
            }
            else if (this.SelectedChart == Application.Current.FindResource("SProductsPerCategory").ToString())
            {
                ShowChartInventory();
            }
            else
            {
                throw new ArgumentException("Selected chart sorting method does not exist in resource");
            }
        }

        private void ShowChartSales()
        {
            CalculateSum();
            List<KeyValuePair<string, int>> salesValue = new List<KeyValuePair<string, int>>();
            foreach (Category category in this.AllCategories)
            {
                int amountPerCategory = 0;
                foreach(SoldProduct product in AllSoldProducts)
                {
                    if(product.date >= DateFrom && product.date <= DateTo && category.id == product.category)
                    {
                        amountPerCategory += (int)product.price;
                    }
                }
                if (amountPerCategory != 0)
                {
                    salesValue.Add(new KeyValuePair<string, int>(category.name, amountPerCategory));
                }
            }
            PieChart = salesValue;
        }

        private void ShowChartItems()
        {
            CalculateSum();
            List<KeyValuePair<string, int>> itemsValue = new List<KeyValuePair<string, int>>();
            foreach (Category category in this.AllCategories)
            {
                int numberOfProducts = 0;
                foreach (SoldProduct product in AllSoldProducts)
                {
                    if (category.id == product.category && product.date >= DateFrom && product.date <= DateTo)
                    {
                        numberOfProducts++;
                    }
                }
                if (numberOfProducts != 0)
                {
                    itemsValue.Add(new KeyValuePair<string, int>(category.name, numberOfProducts));
                }
            }

            PieChart = itemsValue;
        }

        private void ShowChartInventory()
        {
            List<KeyValuePair<string, int>> inventoryValue = new List<KeyValuePair<string, int>>();
            foreach (Category category in AllCategories)
            {
                int numberOfProducts = 0;
                foreach (Product product in AllProducts)
                {
                    if(category.id == product.category /*&& product.date >= dateFrom && product.date <= dateTo*/)
                    {
                        numberOfProducts++;
                    }
                }
                if (numberOfProducts != 0)
                {
                    inventoryValue.Add(new KeyValuePair<string, int>(category.name, numberOfProducts));
                }
            }
            PieChart = inventoryValue;
        }
        
        private List<string> StatCombobox()
        {
            List<string> data = new List<string>();

            data.Add(Application.Current.FindResource("SRevenuePerCategory").ToString());
            data.Add(Application.Current.FindResource("SProductsSoldPerCategory").ToString());
            data.Add(Application.Current.FindResource("SProductsPerCategory").ToString());

            return data;
        }
        #endregion
    }
}
