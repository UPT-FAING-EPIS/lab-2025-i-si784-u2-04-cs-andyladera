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
    private Mock<IDiscountService> discountServiceMock;
    private Mock<ICard> cardMock;
    private Mock<IAddressInfo> addressInfoMock;
    private List<ICartItem> items;

    [SetUp]
    public void Setup()
    {
        cartServiceMock = new Mock<ICartService>();
        paymentServiceMock = new Mock<IPaymentService>();
        shipmentServiceMock = new Mock<IShipmentService>();
        discountServiceMock = new Mock<IDiscountService>();

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

        controller = new CartController(
            cartServiceMock.Object, 
            paymentServiceMock.Object, 
            shipmentServiceMock.Object,
            discountServiceMock.Object
        );
    }

    [Test]
    public void ShouldReturnCharged()
    {
        string expected = "charged";
        cartServiceMock.Setup(c => c.Total()).Returns(100);
        discountServiceMock.Setup(d => d.CalculateDiscount(100)).Returns(10);
        paymentServiceMock.Setup(p => p.Charge(90, cardMock.Object)).Returns(true);

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
        cartServiceMock.Setup(c => c.Total()).Returns(100);
        discountServiceMock.Setup(d => d.CalculateDiscount(100)).Returns(10);
        paymentServiceMock.Setup(p => p.Charge(90, cardMock.Object)).Returns(false);

        // act
        var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

        // assert
        shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Never());
        Assert.That(expected, Is.EqualTo(result));
    }

    [TestCase(100, 10, true, "charged", 1)]
    [TestCase(100, 10, false, "not charged", 0)]
    [TestCase(200, 20, true, "charged", 1)]
    [TestCase(200, 20, false, "not charged", 0)]
    [TestCase(50, 5, true, "charged", 1)]
    [TestCase(50, 5, false, "not charged", 0)]
    public void CheckOut_WithVariousScenarios_ShouldReturnExpectedResult(
        double total, 
        double discount, 
        bool paymentResult, 
        string expectedResult, 
        int expectedShipmentCalls)
    {
        // arrange
        cartServiceMock.Setup(c => c.Total()).Returns(total);
        discountServiceMock.Setup(d => d.CalculateDiscount(total)).Returns(discount);
        paymentServiceMock.Setup(p => p.Charge(total - discount, cardMock.Object)).Returns(paymentResult);

        // act
        var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

        // assert
        Assert.That(expectedResult, Is.EqualTo(result));
        shipmentServiceMock.Verify(
            s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), 
            Times.Exactly(expectedShipmentCalls)
        );
        
        // Verificar que se calculó el descuento
        discountServiceMock.Verify(d => d.CalculateDiscount(total), Times.Once);
        
        // Verificar que se cobró el monto correcto (total - descuento)
        paymentServiceMock.Verify(p => p.Charge(total - discount, cardMock.Object), Times.Once);
    }
}