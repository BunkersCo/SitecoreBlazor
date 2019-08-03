﻿using Foundation.BlazorExtensions;
using Foundation.BlazorExtensions.Services;
using SitecoreBlazorHosted.Shared.Models.Navigation;
using SitecoreBlazorHosted.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feature.Navigation.Repositories
{
    public class NavigationRepository
    {
        private readonly BlazorItemsService _blazorItemsService;
        private readonly BlazorStateMachine _blazorStateMachine;

        public NavigationRepository(BlazorItemsService blazorItemsService, BlazorStateMachine blazorStateMachine)
        {
            _blazorItemsService = blazorItemsService;
            _blazorStateMachine = blazorStateMachine;
        }

        public async Task<List<NavigationItem>> GetBreadcrumb()
        {


            var items = await GetNavigationHierarchy();
            items.Reverse();
            for (var i = 0; i < items.Count - 1; i++)
            {
                items[i].Level = i;
            }

            return items;



        }

        private async Task<List<NavigationItem>> GetNavigationHierarchy()
        {


            var routeId = _blazorStateMachine.RouteId;

            var menuItems = new List<NavigationItem>();

            foreach (var item in await GetMenu())
            {
                menuItems.Add(item);

                if (item.Children != null && item.Children.Any())
                {
                    menuItems.AddRange(item.Children);
                }


            }

            List<NavigationItem> result = new List<NavigationItem>();


            while (!string.IsNullOrWhiteSpace(routeId))
            {
                NavigationItem currentItem = menuItems.FirstOrDefault(i => i.Item.Id == routeId);

                if (currentItem != null)
                {
                    result.Add(currentItem);
                }

                routeId = currentItem?.Item?.Parent?.Id;

            }

            return result;


        }

        private NavigationItem CreateNavigationItem(IBlazorItem item)
        {

            return new NavigationItem
            {
                Item = item,
                Url = item.Url,
                Children = item.HasChildren ? GetChildNavigationItems(item) : null
            };
        }

        private List<NavigationItem> GetChildNavigationItems(IBlazorItem item)
        {
            List<NavigationItem> children = new List<NavigationItem>();

            if (!item.HasChildren)
                return children;

            foreach (var child in item.Children)
            {
                if (child == null)
                    continue;

                children.Add(CreateNavigationItem(child));
            }

            return children;
        }

        public Task<List<NavigationItem>> GetMenu()
        {
            string currentLanguage = _blazorStateMachine.Language;
            IBlazorItem rootItem = _blazorItemsService.GetBlazorItemRootMock(currentLanguage);


            List<NavigationItem> list = new List<NavigationItem>
          {
                   new NavigationItem() //Home
                   {
                       Item = rootItem,
                       Url = rootItem.Url,
                       Children = null
                   }
          };

            foreach (IBlazorItem item in rootItem.Children)
            {

                if (item == null)
                    continue;

                list.Add(
                         CreateNavigationItem(item)
                         );
            }


            return Task.FromResult<List<NavigationItem>>(list);


        }
    }
}
