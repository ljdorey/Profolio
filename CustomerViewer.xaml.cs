//<!--  -Copyright 2021 Alexandra Dorey / Wellington Fabrics  -->﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WFInventory.Cloud;

using WFInventory.ViewModels;
using FSCommon;

namespace WFInventory
{
    /// <summary>
    /// Interaction logic for CustomerViewer.xaml
    /// </summary>
    public partial class CustomerViewer : Window
    {
        wf_CustomerDocumentViewModel Customer;

        ICollectionView dataViewOrderLineItem;

        public CustomerViewer(wf_CustomerDocumentViewModel cust)
        {
            //if (cust.IsADuplicateCustomerRecord)
            //{
            //    Customer = (wf_CustomerDocumentViewModel)FS.GetVM(FS.GetNode<wf_Customer>(cust.ActualCustomerId));
            //}
            //else
            //{
                Customer = cust;
            //}
            InitializeComponent();
            DataContext = Customer;
            cust.InitFromUIThread();
            AddressDisplay.DataContext = this;
        }

        DateTime currentOrderTime;
        string currentOrderId;         

        public bool OrderLineItemFilter(object o)
        {
            if (currentOrderTime == null) return false;
            if ((o as wf_OrderLineItemDocumentViewModel).OrderId == currentOrderId) return true;
            return false;
        }

        public bool EnableAddressEditFields { get => false; }
    
        private void AddressListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddressDisplay.DataContext = (sender as ListBox).SelectedItem;
            Customer.EnableAddressEditFields = true;
        }

        private void OrderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AddressListBox_Loaded(object sender, RoutedEventArgs e)
        {

        }

       
        private void ShipmentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShipmentInfoBox.DataContext = ShipmentListBox.SelectedItem;
        }

        private void ShipmentListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ShipmentListBox.SelectedItem is wf_ShipmentDocumentViewModel)
                WindowManager.OpenWindow(ShipmentListBox.SelectedItem);
        }

        private void PickSku(object sender, RoutedEventArgs e)
        {
            ObservableCollection<iDocumentViewModel> productswithstock = new ObservableCollection<iDocumentViewModel>();
            foreach (wf_ProductSkuDocumentViewModel sku in FS.GetTypeTable<wf_FabricCut>().ViewModels)
            {
                if(sku.Parent.GetType() == typeof(wf_Product))
                {
                    productswithstock.Add(sku);
                }
            }

            DocumentSelector ds = new DocumentSelector(productswithstock);
            bool res = ds.ShowDialog() ?? false;
            if(res)
            {
                //TODO add as orderLineItem...
            }
        }
    }
}
