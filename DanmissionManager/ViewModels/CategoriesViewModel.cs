﻿using DanmissionManager.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DanmissionManager.ViewModels
{
    class CategoriesViewModel : BaseViewModel
    {
        public CategoriesViewModel(Popups popupService) : base(popupService)
        {
            //instantiate member variables
            this.CreatedCategory = new Category();
            this.CreatedStandardprice = new Standardprice();
            
            this.CommandGetCategories = new RelayCommand(FetchCategories);
            this.CommandAddCategory = new RelayCommand(AddCategory, CanAddCategory);
            this.CommandAddSubCategory = new RelayCommand(AddSubCategory, CanAddSubCategory);
            this.CommandRemoveCategory = new RelayCommand(RemoveCategory);
            this.CommandRemoveSubCategory = new RelayCommand(RemoveSubCategory);

            FetchCategories();
        }

        #region Properties

        private string _categoryName;
        public string CategoryName
        {
            get { return _categoryName; }
            set{_categoryName = value; OnPropertyChanged("CategoryName");
             IsNameValid(value, nameof(this.CategoryName)); CommandAddCategory.RaiseCanExecuteChanged();}
        }

        private string _subCategoryName;
        public string SubCategoryName
        {
            get { return _subCategoryName; }
            set{_subCategoryName = value; OnPropertyChanged("SubCategoryName");
                IsNameValid(value, nameof(this.SubCategoryName)); CommandAddSubCategory.RaiseCanExecuteChanged();}
        }

        private double _price;
        public double Price
        {
            get { return _price; }
            set{_price = value; OnPropertyChanged("Price");
                IsPriceValid(value, nameof(this.Price)); CommandAddSubCategory.RaiseCanExecuteChanged();}
        }

        private ObservableCollection<Standardprice> _shownSubCategories;
        public ObservableCollection<Standardprice> ShownSubCategories
        {
            get { return _shownSubCategories; }
            set { _shownSubCategories = value; OnPropertyChanged("ShownSubCategories"); }
        }

        private ObservableCollection<Standardprice> _allSubCategories;
        public ObservableCollection<Standardprice> AllSubCategories
        {
            get { return _allSubCategories; }
            set { _allSubCategories = value; OnPropertyChanged("AllSubCategories"); }
        }

        private ObservableCollection<Category> _allCategories;
        public ObservableCollection<Category> AllCategories
        {
            get { return _allCategories; }
            set { _allCategories = value; OnPropertyChanged("AllCategories"); }
        }

        private ObservableCollection<Category> _shownCategories;
        public ObservableCollection<Category> ShownCategories
        {
            get { return _shownCategories; }
            set { _shownCategories = value; OnPropertyChanged("ShownCategories"); }
        }

        private Category _createdCategory;
        public Category CreatedCategory
        {
            get { return _createdCategory; }
            set { _createdCategory = value; OnPropertyChanged("CreatedCategory"); }
        }

        private Standardprice _createdStandardprice;
        public Standardprice CreatedStandardprice
        {
            get { return _createdStandardprice; }
            set { _createdStandardprice = value; OnPropertyChanged("CreatedStandardPrice"); }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set { _selectedCategory = value; OnPropertyChanged("SelectedCategory");
                ChangeCollection(); }
        }

        private Standardprice _selectedSubCategory;
        public Standardprice SelectedSubCategory
        {
            get { return _selectedSubCategory; }
            set { _selectedSubCategory = value; OnPropertyChanged("SelectedSubCategory"); }
        }

        #endregion

        #region CommandProperties

        public RelayCommand CommandGetCategories { get; }
        public RelayCommand CommandAddCategory { get; }
        public RelayCommand CommandAddSubCategory { get; }
        public RelayCommand CommandRemoveCategory { get; }
        public RelayCommand CommandRemoveSubCategory { get; }

        #endregion

        #region Methods

        private void FetchCategories()
        {
            GetCategoriesFromDatabase();
            SortCategoriesAlphabetically();
            ShownCategories = AllCategories;
            ShownSubCategories = new ObservableCollection<Standardprice>();
        }

        private void GetCategoriesFromDatabase()
        {
            try
            {
                using (var ctx = new ServerContext())
                {
                    this.AllCategories = new ObservableCollection<Category>(ctx.Category.ToList());
                    this.AllSubCategories = new ObservableCollection<Standardprice>(ctx.Standardprices.ToList());
                }
            }
            catch (System.Data.DataException)
            {
                PopupService.PopupMessage(Application.Current.FindResource("CouldNotConnectToDatabase").ToString(), Application.Current.FindResource("Error").ToString());
            }
        }

        private void SortCategoriesAlphabetically()
        {
            AllCategories = new ObservableCollection<Category>(AllCategories.OrderBy(cat => cat.FullName));
        }

        private void ChangeCollection()
        {
            List<Standardprice> list = new List<Standardprice>();
            list = this.AllSubCategories.Where(x => this.SelectedCategory.id == x.Parent_id).ToList();
            list = new List<Standardprice>(list.OrderBy(Standardprice => Standardprice.name));

            ObservableCollection<Standardprice> collection = new ObservableCollection<Standardprice>(list);
            this.ShownSubCategories = collection;
        }

        public void AddCategory()
        {
            try
            {
                using (var ctx = new ServerContext())
                {
                    this.CreatedCategory.name = this.CategoryName;
                    ctx.Category.Add(this.CreatedCategory);
                    ctx.SaveChanges();
                }
                AllCategories.Add(CreatedCategory);
                this.CreatedCategory = new Category();
            }
            catch (System.Data.DataException)
            {
                PopupService.PopupMessage(Application.Current.FindResource("CouldNotConnectToDatabase").ToString(), Application.Current.FindResource("Error").ToString());
            }
        }

        public bool CanAddCategory()
        {
            var errorsCategoryName = GetErrors(nameof(this.CategoryName));

            if (errorsCategoryName != null)
            {
                return false;
            }
            return true;
        }

        public void AddSubCategory()
        {
            this.CreatedStandardprice.Parent_id = SelectedCategory.id;
            this.CreatedStandardprice.CorrespondingCategoryString = SelectedCategory.name;

            try
            {
                using (var ctx = new ServerContext())
                {
                    this.CreatedStandardprice.name = this.SubCategoryName;
                    this.CreatedStandardprice.standardprice = this.Price;
                    ctx.Standardprices.Add(this.CreatedStandardprice);
                    ctx.SaveChanges();
                }
                this.AllSubCategories.Add(CreatedStandardprice);
                this.ShownSubCategories.Add(CreatedStandardprice);
                this.CreatedStandardprice = new Standardprice();
            }
            catch (System.Data.DataException)
            {
                PopupService.PopupMessage(Application.Current.FindResource("CouldNotConnectToDatabase").ToString(), Application.Current.FindResource("Error").ToString());
            }
        }

        public bool CanAddSubCategory()
        {
            var errorsSubCategoryName = GetErrors(nameof(this.SubCategoryName));
            var errorsPrice = GetErrors(nameof(this.Price));

            if (this.SelectedCategory != null && errorsSubCategoryName != null && errorsPrice != null)
            {
                return false;
            }

            return true;
        }

        private void RemoveCategory()
        {
            try
            {
                using (var ctx = new ServerContext())
                {
                    List<Category> categoryList = ctx.Category.Where(x => x.id.CompareTo(SelectedCategory.id) == 0).ToList();
                    Category category = categoryList.First();

                    ctx.Category.Remove(category);
                    ctx.SaveChanges();
                    RemoveSubCategories();
                }
                this.AllCategories.Remove(this.SelectedCategory);
                this.ShownSubCategories = new ObservableCollection<Standardprice>();
            }
            catch (System.Data.DataException)
            {
                PopupService.PopupMessage(Application.Current.FindResource("CouldNotConnectToDatabase").ToString(), Application.Current.FindResource("Error").ToString());
            }
        }

        //help method for RemoveCategory
        private void RemoveSubCategories()
        {
            try
            {
                using (var ctx = new ServerContext())
                {
                    List<Standardprice> subCategoryList = ctx.Standardprices.Where(x => x.Parent_id.CompareTo(SelectedCategory.id) == 0).ToList();
                    foreach (Standardprice subcategory in subCategoryList)
                    {
                        Standardprice tmp = new Standardprice();
                        tmp = subcategory;
                        ctx.Standardprices.Remove(tmp);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (System.Data.DataException)
            {
                PopupService.PopupMessage(Application.Current.FindResource("CouldNotConnectToDatabase").ToString(), Application.Current.FindResource("Error").ToString());
            }
        }

        private void RemoveSubCategory()
        {
            try
            {
                using (var ctx = new ServerContext())
                {
                    List<Standardprice> subCategoryList = ctx.Standardprices.Where(x => x.id.CompareTo(SelectedSubCategory.id) == 0).ToList();
                    Standardprice subCategory = subCategoryList.First();

                    ctx.Standardprices.Remove(subCategory);
                    ctx.SaveChanges();
                }
                this.AllSubCategories.Remove(this.SelectedSubCategory);
                this.ShownSubCategories.Remove(this.SelectedSubCategory);
            }
            catch (System.Data.DataException)
            {
                PopupService.PopupMessage(Application.Current.FindResource("CouldNotConnectToDatabase").ToString(), Application.Current.FindResource("Error").ToString());
            }
        }

        #endregion
    }
}
