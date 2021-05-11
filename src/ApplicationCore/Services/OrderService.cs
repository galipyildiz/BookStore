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
    public class OrderService : IOrderService
    {
        private readonly IAsyncRepository<Order> _orderRepository;
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly IAsyncRepository<Product> _productRepository;

        //burrdaki enjeksiyonu .net core kendi yapıyor. 
        public OrderService(IAsyncRepository<Order> orderRepository, IAsyncRepository<Basket> basketRepository, IAsyncRepository<Product> productRepository)
        {
            _orderRepository = orderRepository;
            _basketRepository = basketRepository;
            _productRepository = productRepository;
        }
        public async Task CreateOrderAsync(int basketId, Address shippingAddress)
        {
            var spec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.FirstOrDefaultAsync(spec);
            var specProducts = new ProductsSpecification(basket.BasketItems.Select(x=>x.ProductId).ToArray());
            var products = await _productRepository.ListAsync(specProducts);
            Order order = new Order()
            {
                OrderDate = DateTimeOffset.Now,
                ShiptoAddress = shippingAddress,
                BuyerId = basket.BuyerId,
                OrderItems = basket.BasketItems.Select(x => 
                {
                    var product = products.First(p => p.Id == x.ProductId);
                    return new OrderItem()
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        PictureUri = product.PictureUri,
                        Price = product.Price,
                        ProductName = product.Name
                    };
                }).ToList()
            };
            await _orderRepository.AddAsync(order);
        }
    }
}
