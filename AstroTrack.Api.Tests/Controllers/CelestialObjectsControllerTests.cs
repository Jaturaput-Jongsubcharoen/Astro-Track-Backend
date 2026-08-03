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

    [Fact]
    public async Task Post_ReturnsCreatedAtAction_WhenCreateSucceeds()
    {
        var request = CreateCreateDto(5010);
        var createdDto = new CelestialObjectDto { ObjectId = 5010, ObjectName = "Created", Category = "Planet" };

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.CreateAsync(request))
            .ReturnsAsync(CelestialObjectMutationResult.Success(createdDto));

        var controller = new CelestialObjectsController(serviceMock.Object);

        var actionResult = await controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(nameof(CelestialObjectsController.GetById), createdResult.ActionName);
        Assert.Equal(createdDto, createdResult.Value);
    }

    [Fact]
    public async Task Post_ReturnsConflict_WhenCreateIsDuplicate()
    {
        var request = CreateCreateDto(7);

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.CreateAsync(request))
            .ReturnsAsync(CelestialObjectMutationResult.Duplicate("duplicate"));

        var controller = new CelestialObjectsController(serviceMock.Object);

        var actionResult = await controller.Create(request);

        Assert.IsType<ConflictObjectResult>(actionResult);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var serviceMock = new Mock<ICelestialObjectService>();
        var controller = new CelestialObjectsController(serviceMock.Object);
        controller.ModelState.AddModelError("ObjectName", "ObjectName is required.");

        var actionResult = await controller.Create(CreateCreateDto(5050));

        Assert.IsType<BadRequestObjectResult>(actionResult);
        serviceMock.Verify(service => service.CreateAsync(It.IsAny<CreateCelestialObjectDto>()), Times.Never);
    }

    [Fact]
    public async Task Put_ReturnsOk_WhenUpdateSucceeds()
    {
        var request = CreateUpdateDto("Updated Name");
        var updatedDto = new CelestialObjectDto { ObjectId = 6010, ObjectName = "Updated Name", Category = "Moon" };

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.UpdateAsync(6010, request))
            .ReturnsAsync(CelestialObjectMutationResult.Success(updatedDto));

        var controller = new CelestialObjectsController(serviceMock.Object);

        var actionResult = await controller.Update(6010, request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(updatedDto, okResult.Value);
    }

    [Fact]
    public async Task Put_ReturnsNotFound_WhenObjectMissing()
    {
        var request = CreateUpdateDto("Missing");

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.UpdateAsync(8888, request))
            .ReturnsAsync(CelestialObjectMutationResult.NotFound("missing"));

        var controller = new CelestialObjectsController(serviceMock.Object);

        var actionResult = await controller.Update(8888, request);

        Assert.IsType<NotFoundResult>(actionResult);
    }

    [Fact]
    public async Task Put_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var serviceMock = new Mock<ICelestialObjectService>();
        var controller = new CelestialObjectsController(serviceMock.Object);
        controller.ModelState.AddModelError("Category", "Category is required.");

        var actionResult = await controller.Update(6011, CreateUpdateDto("Invalid"));

        Assert.IsType<BadRequestObjectResult>(actionResult);
        serviceMock.Verify(service => service.UpdateAsync(It.IsAny<long>(), It.IsAny<UpdateCelestialObjectDto>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteSucceeds()
    {
        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.DeleteAsync(7001))
            .ReturnsAsync(CelestialObjectMutationResult.Success());

        var controller = new CelestialObjectsController(serviceMock.Object);

        var actionResult = await controller.Delete(7001);

        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenObjectMissing()
    {
        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.DeleteAsync(7002))
            .ReturnsAsync(CelestialObjectMutationResult.NotFound("missing"));

        var controller = new CelestialObjectsController(serviceMock.Object);

        var actionResult = await controller.Delete(7002);

        Assert.IsType<NotFoundResult>(actionResult);
    }

    [Fact]
    public async Task Put_PassesRequestedLongIdToService()
    {
        const long requestedId = 6543210;
        var request = CreateUpdateDto("IdCheck");

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.UpdateAsync(requestedId, request))
            .ReturnsAsync(CelestialObjectMutationResult.NotFound("missing"));

        var controller = new CelestialObjectsController(serviceMock.Object);

        await controller.Update(requestedId, request);

        serviceMock.Verify(service => service.UpdateAsync(requestedId, request), Times.Once);
    }

    [Fact]
    public async Task Delete_PassesRequestedLongIdToService()
    {
        const long requestedId = 7654321;

        var serviceMock = new Mock<ICelestialObjectService>();
        serviceMock
            .Setup(service => service.DeleteAsync(requestedId))
            .ReturnsAsync(CelestialObjectMutationResult.NotFound("missing"));

        var controller = new CelestialObjectsController(serviceMock.Object);

        await controller.Delete(requestedId);

        serviceMock.Verify(service => service.DeleteAsync(requestedId), Times.Once);
    }

    private static CreateCelestialObjectDto CreateCreateDto(long objectId)
    {
        return new CreateCelestialObjectDto
        {
            ObjectId = objectId,
            ObjectName = "New Object",
            Category = "Planet",
            DistanceLightYears = 2.0m,
            DiscoveryDate = new DateTime(2024, 1, 1),
            InSolarSystem = "Y",
            HabitabilityScore = 5.5m,
            SurfaceTemperature = 12.3m,
            Gravity = 9.7m,
            Nitrogen = "Y",
            Oxygen = "Y",
            Co2 = "N",
            SulfuricAcid = "N",
            Hydrogen = "N",
            Helium = "N",
            Methane = "N",
            WaterVapor = "Y",
            Silicates = "Y",
            Iron = "Y",
            Nickel = "N"
        };
    }

    private static UpdateCelestialObjectDto CreateUpdateDto(string name)
    {
        return new UpdateCelestialObjectDto
        {
            ObjectName = name,
            Category = "Moon",
            DistanceLightYears = 0,
            DiscoveryDate = new DateTime(2025, 1, 1),
            InSolarSystem = "Y",
            HabitabilityScore = 4.8m,
            SurfaceTemperature = -120.5m,
            Gravity = 1.60m,
            Nitrogen = "N",
            Oxygen = "N",
            Co2 = "Y",
            SulfuricAcid = "N",
            Hydrogen = "N",
            Helium = "N",
            Methane = "N",
            WaterVapor = "N",
            Silicates = "Y",
            Iron = "Y",
            Nickel = "N"
        };
    }
}
