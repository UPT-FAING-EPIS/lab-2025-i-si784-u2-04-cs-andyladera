using EcommerceApp.Api.Models;
namespace EcommerceApp.Api.Services;
public interface ICartService
{
    double Total();
    IEnumerable<ICartItem> Items();         
}