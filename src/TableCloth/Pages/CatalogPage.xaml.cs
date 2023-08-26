﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using TableCloth.Models.Catalog;
using TableCloth.ViewModels;

namespace TableCloth.Pages
{
    /// <summary>
    /// Interaction logic for CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();
        }

        public MainWindowViewModel ViewModel
            => (MainWindowViewModel)DataContext;

        private static readonly PropertyGroupDescription GroupDescription =
            new PropertyGroupDescription(nameof(CatalogInternetService.CategoryDisplayName));

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(ViewModel.Services);
            view.Filter = SiteCatalog_Filter;

            if (!view.GroupDescriptions.Contains(GroupDescription))
                view.GroupDescriptions.Add(GroupDescription);

            UpdateDetailView(SiteCatalog);
        }

        /*
        private GroupItem FindGroupItemForListViewItem(ListView listView, ListViewItem listViewItem)
        {
            // ListViewItem에 바인딩된 데이터 항목 가져오기
            object dataItem = listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

            // ListView의 CollectionView 가져오기
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(listView.ItemsSource);
            if (collectionView is ListCollectionView listCollectionView)
            {
                // 데이터 항목의 그룹 찾기
                CollectionViewGroup group = listCollectionView.Groups.Cast<CollectionViewGroup>().FirstOrDefault(g => g.Items.Contains(dataItem));

                if (group != null)
                {
                    // GroupItem 컨테이너 찾기
                    GroupItem groupItem = listView.ItemContainerGenerator.ContainerFromItem(group) as GroupItem;
                    return groupItem;
                }
            }

            return null;
        }

        private void ExpandGroupItem(ListView view)
        {
            var item = view.SelectedItem;

            if (item == null)
                return;

            var lvItem = view.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

            if (lvItem != null)
            {
                var groupItem = FindGroupItemForListViewItem(view, lvItem);
                var expander = groupItem.Template.FindName("CategoryExpander", groupItem) as Expander;

                if (expander != null)
                    expander.IsExpanded = true;
            }
        }
        */

        private void UpdateDetailView(ListView view)
        {
            var selectedItems = view?.SelectedItems as IEnumerable<object>;
            var selectionCount = selectedItems?.Count() ?? 0;
            var item = selectedItems.FirstOrDefault();

            if (item == null || selectionCount < 1)
            {
                SelectedItemInstructionTextBlock.Visibility = Visibility.Visible;
                MultipleSelectedItemInstructionTextBlock.Visibility = Visibility.Hidden;
                SelectedItemPropertyGrid.Visibility = Visibility.Hidden;
                return;
            }

            if (selectionCount > 1)
            {
                SelectedItemInstructionTextBlock.Visibility = Visibility.Hidden;
                MultipleSelectedItemInstructionTextBlock.Visibility = Visibility.Visible;
                SelectedItemPropertyGrid.Visibility = Visibility.Hidden;
                return;
            }

            SelectedItemInstructionTextBlock.Visibility = Visibility.Hidden;
            MultipleSelectedItemInstructionTextBlock.Visibility = Visibility.Hidden;
            SelectedItemPropertyGrid.Visibility = Visibility.Visible;

            if (item == null)
            {
                SelectedItemInstructionTextBlock.Visibility = Visibility.Visible;
                MultipleSelectedItemInstructionTextBlock.Visibility = Visibility.Hidden;
                SelectedItemPropertyGrid.Visibility = Visibility.Hidden;
                return;
            }

            SelectedItemPropertyGrid.DataContext = item;
        }

        private bool SiteCatalog_Filter(object item)
        {
            var filterText = SiteCatalogFilter.Text;

            if (string.IsNullOrWhiteSpace(filterText))
                return true;

            var actualItem = item as CatalogInternetService;

            if (actualItem == null)
                return true;

            var splittedFilterText = filterText.Split(new char[] { ',', }, StringSplitOptions.RemoveEmptyEntries);
            var result = false;

            foreach (var eachFilterText in splittedFilterText)
            {
                result |= actualItem.DisplayName.Contains(eachFilterText, StringComparison.OrdinalIgnoreCase)
                    || actualItem.CategoryDisplayName.Contains(eachFilterText, StringComparison.OrdinalIgnoreCase)
                    || actualItem.Url.Contains(eachFilterText, StringComparison.OrdinalIgnoreCase)
                    || actualItem.Packages.Count.ToString().Contains(eachFilterText, StringComparison.OrdinalIgnoreCase)
                    || actualItem.Packages.Any(x => x.Name.Contains(eachFilterText, StringComparison.OrdinalIgnoreCase))
                    || actualItem.Id.Contains(eachFilterText, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        private void SiteCatalogFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(SiteCatalog.ItemsSource).Refresh();
        }

        // https://stackoverflow.com/questions/660554/how-to-automatically-select-all-text-on-focus-in-wpf-textbox

        private void SiteCatalogFilter_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Fixes issue when clicking cut/copy/paste in context menu
            if (SiteCatalogFilter.SelectionLength < 1)
                SiteCatalogFilter.SelectAll();
        }

        private void SiteCatalogFilter_LostTouchCapture(object sender, TouchEventArgs e)
        {
            // If user highlights some text, don't override it
            if (SiteCatalogFilter.SelectionLength < 1)
                SiteCatalogFilter.SelectAll();

            // further clicks will not select all
            SiteCatalogFilter.LostTouchCapture -= SiteCatalogFilter_LostTouchCapture;
        }

        private void SiteCatalogFilter_LostMouseCapture(object sender, MouseEventArgs e)
        {
            // If user highlights some text, don't override it
            if (SiteCatalogFilter.SelectionLength < 1)
                SiteCatalogFilter.SelectAll();

            // further clicks will not select all
            SiteCatalogFilter.LostMouseCapture -= SiteCatalogFilter_LostMouseCapture;
        }

        private void SiteCatalogFilter_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // once we've left the TextBox, return the select all behavior
            SiteCatalogFilter.LostMouseCapture += SiteCatalogFilter_LostMouseCapture;
            SiteCatalogFilter.LostTouchCapture += SiteCatalogFilter_LostTouchCapture;
        }

        private void SiteCatalog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDetailView(SiteCatalog);
        }

        private void SiteCatalog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(ListBoxItem))
                SiteCatalog.UnselectAll();
        }

        private void SiteCatalog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItems = SiteCatalog?.SelectedItems as IEnumerable<object>;

            if (selectedItems == null || selectedItems.Count() < 1)
                return;

            NavigationService.Navigate(
                new Uri("Pages/DetailPage.xaml", UriKind.Relative),
                selectedItems);
        }
    }
}
