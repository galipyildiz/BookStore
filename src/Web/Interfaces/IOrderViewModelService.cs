﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.ViewModels;

namespace Web.Interfaces
{
    public interface IOrderViewModelService
    {
        Task<List<OrderViewModel>> ListOrdersAsync();
        Task<List<OrderViewModel>> ListOrderAllAsync();
        Task<List<OrderItemViewModel>> ListOrderItemsAsync(int orderId);
        string GetBuyerId();
    }
}
