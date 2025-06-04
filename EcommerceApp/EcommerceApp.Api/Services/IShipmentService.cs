using EcommerceApp.Api.Models;
namespace EcommerceApp.Api.Services;
public interface IShipmentService
{
    void Ship(IAddressInfo info, IEnumerable<ICartItem> items);
}