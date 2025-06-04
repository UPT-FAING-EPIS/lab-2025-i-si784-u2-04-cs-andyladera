using EcommerceApp.Api.Models;
namespace EcommerceApp.Api.Services;
public interface IPaymentService
{
    bool Charge(double total, ICard card);      
}