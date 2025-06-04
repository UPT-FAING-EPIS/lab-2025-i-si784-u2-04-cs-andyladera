using EcommerceApp.Api.Controllers;
using EcommerceApp.Api.Models;
using EcommerceApp.Api.Services;
using Moq;

namespace EcommerceApp.Tests;
public class ControllerTests
{
      private CartController controller;
      private Mock<IPaymentService> paymentServiceMock;
      private Mock<ICartService> cartServiceMock;

      private Mock<IShipmentService> shipmentServiceMock;
      private Mock<ICard> cardMock;
      private Mock<IAddressInfo> addressInfoMock;
      private List<ICartItem> items;

      [SetUp]
      public void Setup()
      {
          
          cartServiceMock = new Mock<ICartService>();
          paymentServiceMock = new Mock<IPaymentService>();
          shipmentServiceMock = new Mock<IShipmentService>();

          // arrange
          cardMock = new Mock<ICard>();
          addressInfoMock = new Mock<IAddressInfo>();

          // 
          var cartItemMock = new Mock<ICartItem>();
          cartItemMock.Setup(item => item.Price).Returns(10);

          items = new List<ICartItem>()
          {
              cartItemMock.Object
          };

          cartServiceMock.Setup(c => c.Items()).Returns(items.AsEnumerable());

          controller = new CartController(cartServiceMock.Object, paymentServiceMock.Object, shipmentServiceMock.Object);
      }

      [Test]
      public void ShouldReturnCharged()
      {
          string expected = "charged";
          paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(true);

          // act
          var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

          // assert
          shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Once());

          Assert.That(expected, Is.EqualTo(result));
      }

      [Test]
      public void ShouldReturnNotCharged() 
      {
          string expected = "not charged";
          paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(false);

          // act
          var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

          // assert
          shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Never());
          Assert.That(expected, Is.EqualTo(result));
      }    
}