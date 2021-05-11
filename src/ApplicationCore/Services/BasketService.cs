using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class BasketService : IBasketService
    {
        private readonly IAsyncRepository<BasketItem> _basketItemRepository;
        private readonly IAsyncRepository<Basket> _basketRepository;

        public BasketService(IAsyncRepository<BasketItem> basketItemRepository, IAsyncRepository<Basket> basketRepository)
        {
            _basketItemRepository = basketItemRepository;
            _basketRepository = basketRepository;
        }
        public async Task AddItemToBasket(int basketId, int productId, int quantity)
        {
            if (quantity < 1) throw new ArgumentException("Quantity can not be zero or negative number");
            var spec = new BasketItemSpecification(basketId, productId);
            var basketItem = await _basketItemRepository.FirstOrDefaultAsync(spec);

            if (basketItem != null)
            {
                basketItem.Quantity += quantity;
                await _basketItemRepository.UpdateAsync(basketItem);
            }
            else
            {
                basketItem = new BasketItem() { BasketId = basketId, ProductId = productId, Quantity = quantity };
                await _basketItemRepository.AddAsync(basketItem);
            }
        }

        public async Task<int> BasketItemCount(int basketId)
        {
            var spec = new BasketItemSpecification(basketId);
            return await _basketItemRepository.CountAsync(spec);
        }

        public async Task DeleteBasketAsync(int basketId)
        {
            var basket = await _basketRepository.GetByIdAsync(basketId);
            await _basketRepository.DeleteAsync(basket);
        }

        public async Task DeleteBasketItem(int basketId, int basketItemId)
        {
            var spec = new ManageBasketItemSpecification(basketId, basketItemId);
            var item = await _basketItemRepository.FirstOrDefaultAsync(spec);
            await _basketItemRepository.DeleteAsync(item);
        }

        public async Task TransferBasketAsync(string anonymousId, string userId)
        {
            var specAnon = new BasketWithItemsSpecification(anonymousId);//anonim id ile sepet var mi 
            var basketAnon = await _basketRepository.FirstOrDefaultAsync(specAnon);
            if (basketAnon == null || !basketAnon.BasketItems.Any())//yoksa çık
                return;
            
            var specUser = new BasketWithItemsSpecification(userId);
            var basketUser = await _basketRepository.FirstOrDefaultAsync(specUser);
            if (basketUser == null)//kullanıcı sepeti yoksa sepet olustur
                basketUser = await _basketRepository.AddAsync(new Basket() { BuyerId = userId });

            foreach (BasketItem itemAnon in basketAnon.BasketItems)
            {
                var itemUser = basketUser.BasketItems.FirstOrDefault(x => x.ProductId == itemAnon.ProductId);

                if (itemUser == null)//userda o ürün yoksa
                {
                    basketUser.BasketItems.Add(new BasketItem() //ekle
                    { 
                        ProductId = itemAnon.ProductId, 
                        Quantity = itemAnon.Quantity 
                    });
                }
                else
                {
                    itemUser.Quantity += itemAnon.Quantity;//varsa mikttarını artır.
                    await _basketItemRepository.UpdateAsync(itemUser);//kaldırılabilir
                }
            }
            await _basketRepository.UpdateAsync(basketUser);//sepeti güncelle en son
            await _basketRepository.DeleteAsync(basketAnon);//en sonda anonim sepeti sil.
        }

        public async Task UpdateBasketItem(int basketId, int basketItemId, int quantity)
        {
            if (quantity < 1) throw new Exception("The quantity cannot be less than 1.");
            var spec = new ManageBasketItemSpecification(basketId, basketItemId);
            var item = await _basketItemRepository.FirstOrDefaultAsync(spec);
            item.Quantity = quantity;
            await _basketItemRepository.UpdateAsync(item);
        }
    }
}
