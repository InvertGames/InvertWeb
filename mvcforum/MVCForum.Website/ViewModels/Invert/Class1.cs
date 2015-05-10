using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.ViewModels
{
    public class UnityInvoicesViewModel
    {
        public UnityInvoice[] LinkedInvoices { get; set; }
    }

    public class PageSectionsListViewModel
    {
        public PageSectionsListViewModel(string listName)
        {
            ListName = listName;
        }

        public string ListName { get; set; }
    }
}