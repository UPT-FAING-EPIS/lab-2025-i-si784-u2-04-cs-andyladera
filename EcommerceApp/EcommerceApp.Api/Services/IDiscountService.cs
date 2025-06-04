using EcommerceApp.Api.Models;

namespace EcommerceApp.Api.Services;

public interface IDiscountService
{
    double CalculateDiscount(double total);
}