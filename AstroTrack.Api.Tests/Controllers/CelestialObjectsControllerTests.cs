using AstroTrack.Api.Controllers;
using AstroTrack.Api.DTOs.CelestialObjects;
using AstroTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AstroTrack.Api.Tests.Controllers;

public class CelestialObjectsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOkObjectResult()
    {
        var serviceResult = new List<CelestialObjectDto>
        {
            new() { ObjectId = 1, ObjectName = "Earth", Category = "Planet" }
        };

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(serviceResult);

        var controller = new CelestialObjectsController(serviceMock.Object);

        IActionResult actionResult = await controller.GetAll();

        Assert.IsType<OkObjectResult>(actionResult);
        serviceMock.Verify(service => service.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsServiceResult()
    {
        var serviceResult = new List<CelestialObjectDto>
        {
            new() { ObjectId = 1, ObjectName = "Earth", Category = "Planet" },
            new() { ObjectId = 2, ObjectName = "Mars", Category = "Planet" }
        };

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(serviceResult);

        var controller = new CelestialObjectsController(serviceMock.Object);

        var actionResult = await controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var value = Assert.IsAssignableFrom<IEnumerable<CelestialObjectDto>>(okResult.Value);

        Assert.Same(serviceResult, value);
    }

    [Fact]
    public async Task GetById_ReturnsOkObjectResult_WhenObjectExists()
    {
        var dto = new CelestialObjectDto
        {
            ObjectId = 10,
            ObjectName = "Europa",
            Category = "Moon"
        };

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.GetByIdAsync(10))
            .ReturnsAsync(dto);

        var controller = new CelestialObjectsController(serviceMock.Object);

        IActionResult actionResult = await controller.GetById(10);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Same(dto, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundResult_WhenObjectDoesNotExist()
    {
        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.GetByIdAsync(999))
            .ReturnsAsync((CelestialObjectDto?)null);

        var controller = new CelestialObjectsController(serviceMock.Object);

        IActionResult actionResult = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(actionResult);
    }

    [Fact]
    public async Task GetById_PassesRequestedIdToService()
    {
        const long requestedId = 1234;

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.GetByIdAsync(requestedId))
            .ReturnsAsync((CelestialObjectDto?)null);

        var controller = new CelestialObjectsController(serviceMock.Object);

        await controller.GetById(requestedId);

        serviceMock.Verify(service => service.GetByIdAsync(requestedId), Times.Once);
    }
}
