using EcommerceApp.Api.Models;
using EcommerceApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CartController
{
    private readonly ICartService _cartService;
    private readonly IPaymentService _paymentService;
    private readonly IShipmentService _shipmentService;
    private readonly IDiscountService _discountService;
    
    public CartController(
      ICartService cartService,
      IPaymentService paymentService,
      IShipmentService shipmentService,
      IDiscountService discountService
    ) 
    {
      _cartService = cartService;
      _paymentService = paymentService;
      _shipmentService = shipmentService;
      _discountService = discountService;
    }

    [HttpPost]
    public string CheckOut(ICard card, IAddressInfo addressInfo) 
    {
        var total = _cartService.Total();
        var discount = _discountService.CalculateDiscount(total);
        var finalTotal = total - discount;
        
        var result = _paymentService.Charge(finalTotal, card);
        if (result)
        {
            _shipmentService.Ship(addressInfo, _cartService.Items());
            return "charged";
        }
        else {
            return "not charged";
        }
    }    
}