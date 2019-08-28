using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Services
{   //there could be other types of basket services  so like the IRepository interface, we follow that same pattern
    public class BasketService : IBasketService
    {
        //1st look in the cookie for a basket id using http context

        IRepository<Product> productContext;
        IRepository<Basket> basketContext;

        public const string BasketSessionName = "eCommerceBasket"; //used to define/sign any modifications

        public BasketService(IRepository<Product> ProductContext, IRepository<Basket> BasketContext)
        {
            this.basketContext = BasketContext;
            this.productContext = ProductContext;
        }

        //search inside the cookie

        private Basket GetBasket(HttpContextBase httpContext, bool createIfNull)
        {
            HttpCookie cookie = httpContext.Request.Cookies.Get(BasketSessionName);
            Basket basket = new Basket();
            if (cookie != null)
            {
                string basketId = cookie.Value;
                if (!string.IsNullOrEmpty(basketId))
                {
                    basket = basketContext.Find(basketId);
                } else
                {
                    if (createIfNull)
                    {
                        basket = CreateNewBasket(httpContext);
                    }
                }
            } else
            {
                if (createIfNull)
                {
                    basket = CreateNewBasket(httpContext);
                }
            }

            return basket;

        }

        private Basket CreateNewBasket(HttpContextBase httpContext)
        {
            Basket basket = new Basket();
            basketContext.Insert(basket);
            basketContext.Commit();

            HttpCookie cookie = new HttpCookie(BasketSessionName);
            cookie.Value = basket.Id;
            cookie.Expires = DateTime.Now.AddDays(1);
            httpContext.Response.Cookies.Add(cookie); //response since it has to do with the user

            return basket;
        }

        public void AddToBasket(HttpContextBase httpContext, string productId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                item = new BasketItem()
                {
                    BasketId = basket.Id,
                    ProductId = productId,
                    Quantity = 1,
                };

                basket.BasketItems.Add(item);
            } else
            {
                ++item.Quantity; //no need toupdate because entityfw will take care of updating the value
            }

            basketContext.Commit();
        }

        public void RemoveFromBasket(HttpContextBase httpContext, string itemId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.Id == itemId);

            if (item != null)
            {
                basket.BasketItems.Remove(item);
                basketContext.Commit();
            }
        }

        public List<BasketItemViewModel> GetBasketItems(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);

            if (basket != null)
            { 
                var results = (from b in basket.BasketItems                
                               join p in productContext.Collection()        // copies all the data from both the basket
                              on b.ProductId equals p.Id                    // and product by first finding everything
                              select new BasketItemViewModel() {           
                                  Id = b.Id,                                //then creates a new view model with the following data
                                  Quantity = b.Quantity,                    //creating a new list of basket and product data combined 
                                  ProductName = p.Name,
                                  Image = p.Image,
                                  Price = p.Price,
                                }).ToList();
                return results;
            }
            else
            {
                return new List<BasketItemViewModel>();                     //since there is no basket
            }
        }

        public BasketSummaryViewModel GetBasketSummary(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);
            BasketSummaryViewModel model = new BasketSummaryViewModel(0, 0);

            if(basket != null)
            {   //the question mark means that the value could be null 
                int? basketCount = (from item in basket.BasketItems  //querying tha data 
                                    select item.Quantity).Sum();     //counting all items                
                
                decimal? basketTotal = (from item in basket.BasketItems  
                                        join p in productContext.Collection() on item.ProductId equals p.Id
                                        select item.Quantity * p.Price).Sum();

                model.BasketCount = basketCount ?? 0;   //if there is a basket count return that vaue, if null, return 0
                model.BasketTotal = basketTotal ?? decimal.Zero;     //since we need a decimal, better specify it like this 
                                                                     // so we can have a well defined 0           
                return model;                                         
            } else
            {
                return model;
            }
        }
    }
}

