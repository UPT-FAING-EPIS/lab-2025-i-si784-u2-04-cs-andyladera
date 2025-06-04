```mermaid
classDiagram

class ICartService
ICartService : +Total() Double
ICartService : +Items() IEnumerable~ICartItem~

class IDiscountService
IDiscountService : +CalculateDiscount() Double

class IPaymentService
IPaymentService : +Charge() Boolean

class IShipmentService
IShipmentService : +Ship() Void

class IAddressInfo
IAddressInfo : +String Street
IAddressInfo : +String Address
IAddressInfo : +String City
IAddressInfo : +String PostalCode
IAddressInfo : +String PhoneNumber

class ICard
ICard : +String CardNumber
ICard : +String Name
ICard : +DateTime ValidTo

class ICartItem
ICartItem : +String ProductId
ICartItem : +Int Quantity
ICartItem : +Double Price

class CartController
CartController : +CheckOut() String



```
