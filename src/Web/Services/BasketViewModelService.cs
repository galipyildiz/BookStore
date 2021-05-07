﻿using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Specifications;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Web.Interfaces;
using Web.ViewModels;

namespace Web.Services
{
    public class BasketViewModelService : IBasketViewModelService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly IBasketService _basketService;
        private readonly IAsyncRepository<Product> _productRepository;

        public BasketViewModelService(IHttpContextAccessor httpContextAccessor, IAsyncRepository<Basket> basketRepository, IBasketService basketService, IAsyncRepository<Product> productRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _basketRepository = basketRepository;
            _basketService = basketService;
            _productRepository = productRepository;
        }

        public async Task<BasketItemsCountViewModel> GetBasketItemsCountViewModel(int basketId)
        {
            return new BasketItemsCountViewModel()
            {
                BasketItemsCount = await _basketService.BasketItemCount(basketId)
            };
        }

        public async Task<BasketViewModel> GetBasketViewModel()
        {
            int basketId = await GetOrCreateBasketIdAsync();
            var spec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.FirstOrDefaultAsync(spec);
            var productIds = basket.BasketItems.Select(x => x.ProductId).ToArray();
            var specProducts = new ProductsSpecification(productIds);
            var products = await _productRepository.ListAsync(specProducts);

            var basketItems = new List<BasketItemViewModel>();
            foreach (var item in basket.BasketItems)
            {
                var product = products.First(x => x.Id == item.ProductId);
                basketItems.Add(new BasketItemViewModel()
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ProductName = product.Name,
                    Price = product.Price,
                    PictureUri = product.PictureUri
                });
            }
            return new BasketViewModel()
            {
                Items = basketItems,
                BasketId = basketId,
                BuyerId = basket.BuyerId
            };
        }

        public async Task<int> GetOrCreateBasketIdAsync()
        {
            var buyerId = GetOrCreateBuyerId();
            var spec = new BasketSpecification(buyerId);
            var basket = await _basketRepository.FirstOrDefaultAsync(spec);

            if (basket == null) //sepet yoksa oluştur.
            {
                basket = new Basket() { BuyerId = buyerId };
                basket = await _basketRepository.AddAsync(basket);
            }
            return basket.Id;
        }

        public string GetOrCreateBuyerId()
        {
            var context = _httpContextAccessor.HttpContext;
            var user = context.User;

            if (user.Identity.IsAuthenticated)
            {
                return user.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                if (context.Request.Cookies.ContainsKey(Constans.BASKET_COOKIE_NAME))//cookie var mı 
                {
                    return context.Request.Cookies[Constans.BASKET_COOKIE_NAME]; //varsa o cookiye döndür
                }
                else
                {
                    string newBuyerId = Guid.NewGuid().ToString();
                    var cookieOptions = new CookieOptions()
                    {
                        IsEssential = true,//uygulamanın çalışması için gerekli mi? Öyleyse rıza metni dışında kalıyor
                        Expires = DateTime.Now.AddYears(10)
                    };
                    context.Response.Cookies.Append(Constans.BASKET_COOKIE_NAME, newBuyerId, cookieOptions);
                    //cookie oluşturup ekledik.
                    return newBuyerId;
                }
            }
        }
    }
}
