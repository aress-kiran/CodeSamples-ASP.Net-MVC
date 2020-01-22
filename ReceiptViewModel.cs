//@filename: ReceiptViewModel.cs
//@description: This page contains following models.
//              1) model for Receipts
//              2) model for receipt items
//              3) model for receipt tags
//              4) model for receipt images
//@aboutauthor: Aress Softwares
//@copyright: Copyright (C) 2017, JustTap
//@created by: Aress Developer

using PagedList;
using System;
using JustTap.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace JustTap.Areas.Admin.Models
{
    public class ReceiptViewModel
    {
        [Key]
        [Display(Name="Receipt Id")]
        public int ReceiptId { get; set; }

        [Display(Name = "Receipt No")]
        public string ReceiptNo { get; set; }

        public Nullable<int> UserId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Billing Date")]
        [Required(ErrorMessage = "Please enter billing date.")]
        public Nullable<System.DateTime> BillingDate { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }

        public Nullable<int> TaxId { get; set; }

        [Display(Name = "Tax Category")]
        public string TaxName { get; set; }

        public string Note { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Please enter amount.")]
        [Display(Name = "Total Bill (£)")]
        public Nullable<decimal> TotalBillAmount { get; set; }

        [Display(Name = "Tax Amount")]
        public Nullable<decimal> TaxAmount { get; set; }

        [Display(Name = "Supplier Name")]
        [Required(ErrorMessage ="Please enter supplier name.")]        
        public string SupplierName { get; set; }

        [Display(Name = "Location Name")]
        [Required(ErrorMessage = "Please enter location name.")]
        public string LocationName { get; set; }

        [Display(Name = "Total Bill")]
        public Nullable<decimal> TotalBill { get; set; }

        public List<ReceiptImageModel> ListOfReceiptImages{get;set;}

        public List<ReceiptItemModel> ListOfReceiptItem { get; set; }

        [Display(Name="Tags")]
        public int TagId { get; set; }

        public Nullable<System.DateTime> CreatedDate { get; set; }

        public Nullable<System.DateTime> ModifiedDate { get; set; }

        public Nullable<bool> IsDeleted { get; set; }

        public IPagedList<GetAllReceiptsListToAdmin_Result> ReceiptsList { get; set; }

        public List<SelectListItem> tagsList { get; set; }

        public int listcount { get; set; }

        public Nullable<int>[] TagIds { get; set; }

        public List<Tags> TagList { get; set; }
    }

    public class Tags
    {
        public int TagId { get; set; }
        public string TagName { get; set; }
    }

    public class ReceiptImageModel
    {
        [Key]
        public int ImageId { get; set; }    

        public string ImagePath { get; set; }

        public byte[] ReceiptImage { get; set; }
    }

    public class ReceiptItemModel
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage ="*")]
      
        public string ItemName { get; set; }

        [Required(ErrorMessage = "*")]        
        public Nullable<int> Quantity { get; set; }

        [Required(ErrorMessage = "*")]       
        public Nullable<decimal> Price { get; set; }

        public Nullable<bool> IsDeleted { get; set; }
    }
}