[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/oHUMAf7N)
[![Open in Codespaces](https://classroom.github.com/assets/launch-codespace-2972f46106e565e64193e422d61a12cf1da4916b45550586e14ef0a7c637dd04.svg)](https://classroom.github.com/open-in-codespaces?assignment_repo_id=19480950)
# SESION DE LABORATORIO N° 04: PRUEBAS DE INTEGRACIÓN CON MOQ

## OBJETIVOS
  * Comprender el funcionamiento de las pruebas de integración utilizando la libreria Moq.

## REQUERIMIENTOS
  * Conocimientos: 
    - Conocimientos básicos de Bash (powershell).
    - Conocimientos básicos de Contenedores (Docker).
  * Hardware:
    - Virtualization activada en el BIOS..
    - CPU SLAT-capable feature.
    - Al menos 4GB de RAM.
  * Software:
    - Windows 10 64bit: Pro, Enterprise o Education (1607 Anniversary Update, Build 14393 o Superior)
    - Docker Desktop 
    - Powershell versión 7.x
    - Net 8 o superior
    - Visual Studio Code

## CONSIDERACIONES INICIALES
  * Clonar el repositorio mediante git para tener los recursos necesarios

## DESARROLLO
1. Iniciar la aplicación Powershell o Windows Terminal en modo administrador 
2. Ejecutar el siguiente comando para crear una nueva solución
```
dotnet new sln -o EcommerceApp
```
3. Acceder a la solución creada y ejecutar el siguiente comando para crear una nueva libreria de clases y adicionarla a la solución actual.
```
cd EcommerceApp
dotnet new webapi -o EcommerceApp.Api
dotnet sln add EcommerceApp.Api
```
4. Ejecutar el siguiente comando para crear un nuevo proyecto de pruebas y adicionarla a la solución actual
```
dotnet new nunit -o EcommerceApp.Tests
dotnet sln add EcommerceApp.Tests
dotnet add EcommerceApp.Tests reference EcommerceApp.Api
dotnet add EcommerceApp.Tests package Moq
```
5. Iniciar Visual Studio Code (VS Code) abriendo el folder de la solución como proyecto. En el proyecto EcommerceApp.Api, crear las carpetas Models y Services.

6. En VS Code, en el proyecto EcommerceApp.Api, dentro de la carpeta Models crear las siguientes interfaces:
> IAddressInfo.cs
```C#
namespace EcommerceApp.Api.Models;
public interface IAddressInfo
{
    public string Street { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string PhoneNumber { get; set; }         
}
```  
> ICard.cs
```C#
namespace EcommerceApp.Api.Models;
public interface ICard
{
    public string CardNumber { get; set; }
    public string Name { get; set; }
    public DateTime ValidTo { get; set; }
}
```
> ICartItem.cs
```C#
namespace EcommerceApp.Api.Models;
public interface ICartItem
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public double Price{ get; set; }
}
```  
7. En VS Code, en el proyecto EcommerceApp.Api, dentro de la carpeta Services crear las siguientes interfaces:
> ICartService.cs
```C#
using EcommerceApp.Api.Models;
namespace EcommerceApp.Api.Services;
public interface ICartService
{
    double Total();
    IEnumerable<ICartItem> Items();         
}
```  
> IPaymentService.cs
```C#
using EcommerceApp.Api.Models;
namespace EcommerceApp.Api.Services;
public interface IPaymentService
{
    bool Charge(double total, ICard card);      
}
```
> IShipmentService.cs
```C#
using EcommerceApp.Api.Models;
namespace EcommerceApp.Api.Services;
public interface IShipmentService
{
    void Ship(IAddressInfo info, IEnumerable<ICartItem> items);
}
```  
8. En VS Code, en el proyecto EcommerceApp.Api, dentro de la carpeta Controllers crear el siguiente controlador:
> CartController.cs
```C#
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
    
    public CartController(
      ICartService cartService,
      IPaymentService paymentService,
      IShipmentService shipmentService
    ) 
    {
      _cartService = cartService;
      _paymentService = paymentService;
      _shipmentService = shipmentService;
    }

    [HttpPost]
    public string CheckOut(ICard card, IAddressInfo addressInfo) 
    {
        var result = _paymentService.Charge(_cartService.Total(), card);
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
```  
9. Luego en el proyecto EcommerceApp.Tests añadir un nuevo archivo CartControllerTests.cs e introducir el siguiente código:
```C#
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
```
8. Abrir un terminal en VS Code (CTRL + Ñ) o vuelva al terminal anteriormente abierto, y ejecutar los comandos:
```Bash
dotnet test --collect:"XPlat Code Coverage"
```
9. El resultado debe ser similar al siguiente. 
```Bash
Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     3, Duration: 12 ms
```
10. Finalmente proceder a verificar la cobertura, dentro del proyecto Primes.Tests se dede haber generado una carpeta o directorio TestResults, en el cual posiblemente exista otra subpcarpeta o subdirectorio conteniendo un archivo con nombre `coverage.cobertura.xml`, si existe ese archivo proceder a ejecutar los siguientes comandos desde la linea de comandos abierta anteriomente, de los contrario revisar el paso 8:
```
dotnet tool install -g dotnet-reportgenerator-globaltool
ReportGenerator "-reports:./*/*/*/coverage.cobertura.xml" "-targetdir:Cobertura" -reporttypes:HTML
```
11. El comando anterior primero proceda instalar una herramienta llamada ReportGenerator (https://reportgenerator.io/) la cual mediante la segunda parte del comando permitira generar un reporte en formato HTML con la cobertura obtenida de la ejecución de las pruebas. Este reporte debe localizarse dentro de una carpeta llamada Cobertura y puede acceder a el abriendo con un navegador de internet el archivo index.htm.

---
## Actividades Encargadas
1. Reescribir los metodos adicionando un servicio de descuento sobre el monto total de la operación y crear un tercer metodo de prueba que combine los dos anteriores utilizando parametros para las pruebas.
2. Utilizar el utilitario ddl2mmd para generar el diagrama de clases (clases.md) del proyecto EcommerceApp.Api.
3. Completar la documentación del Clases, atributos y métodos para luego generar una automatización (publish_docs.yml) que genere la documentación utilizando DocFx y la publique en una Github Page
4. Generar una automatización (publish_cov_report.yml) que: * Compile el proyecto y ejecute las pruebas unitarias, * Genere el reporte de cobertura, * Publique el reporte en Github Page
5. Generar una automatización (release.yml) que: * Genere el nuget con su codigo de matricula como version del componente, * Publique el nuget en Github Packages, * Genere el release correspondiente
