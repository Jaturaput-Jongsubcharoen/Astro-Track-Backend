using AstroTrack.Api.DTOs.CelestialObjects;
using AstroTrack.Api.Models;
using AstroTrack.Api.Repositories;
using AstroTrack.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AstroTrack.Api.Tests.Services;

public class CelestialObjectServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var entities = new List<CelestialObject>
        {
            CreateEntity(objectId: 1, objectName: "Earth"),
            CreateEntity(objectId: 2, objectName: "Mars")
        };

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(entities);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = (await service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Earth", result[0].ObjectName);
        Assert.Equal("Mars", result[1].ObjectName);
        repositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyCollection_WhenRepositoryReturnsNoEntities()
    {
        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(Array.Empty<CelestialObject>());

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.GetAllAsync();

        Assert.Empty(result);
        repositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedDto_WhenEntityExists()
    {
        var entity = CreateEntity(objectId: 7, objectName: "Kepler-452b");

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(7))
            .ReturnsAsync(entity);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.GetByIdAsync(7);

        Assert.NotNull(result);
        Assert.Equal(7, result!.ObjectId);
        Assert.Equal("Kepler-452b", result.ObjectName);
        repositoryMock.Verify(repository => repository.GetByIdAsync(7), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenRepositoryReturnsNull()
    {
        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(99))
            .ReturnsAsync((CelestialObject?)null);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.GetByIdAsync(99);

        Assert.Null(result);
        repositoryMock.Verify(repository => repository.GetByIdAsync(99), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_MapsAllCelestialObjectProperties()
    {
        var discoveryDate = new DateTime(1781, 3, 13);
        var entity = new CelestialObject
        {
            ObjectId = 42,
            ObjectName = "Uranus",
            Category = "Planet",
            DistanceLightYears = 0m,
            DiscoveryDate = discoveryDate,
            InSolarSystem = true,
            HabitabilityScore = 2.5m,
            SurfaceTemperature = -197.2m,
            Gravity = 8.69m,
            Nitrogen = true,
            Oxygen = false,
            Co2 = true,
            SulfuricAcid = false,
            Hydrogen = true,
            Helium = true,
            Methane = true,
            WaterVapor = false,
            Silicates = true,
            Iron = true,
            Nickel = false
        };

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(42))
            .ReturnsAsync(entity);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        CelestialObjectDto? result = await service.GetByIdAsync(42);

        Assert.NotNull(result);
        Assert.Equal(entity.ObjectId, result!.ObjectId);
        Assert.Equal(entity.ObjectName, result.ObjectName);
        Assert.Equal(entity.Category, result.Category);
        Assert.Equal(entity.DistanceLightYears, result.DistanceLightYears);
        Assert.Equal(entity.DiscoveryDate, result.DiscoveryDate);
        Assert.Equal(entity.InSolarSystem, result.InSolarSystem);
        Assert.Equal(entity.HabitabilityScore, result.HabitabilityScore);
        Assert.Equal(entity.SurfaceTemperature, result.SurfaceTemperature);
        Assert.Equal(entity.Gravity, result.Gravity);
        Assert.Equal(entity.Nitrogen, result.Nitrogen);
        Assert.Equal(entity.Oxygen, result.Oxygen);
        Assert.Equal(entity.Co2, result.Co2);
        Assert.Equal(entity.SulfuricAcid, result.SulfuricAcid);
        Assert.Equal(entity.Hydrogen, result.Hydrogen);
        Assert.Equal(entity.Helium, result.Helium);
        Assert.Equal(entity.Methane, result.Methane);
        Assert.Equal(entity.WaterVapor, result.WaterVapor);
        Assert.Equal(entity.Silicates, result.Silicates);
        Assert.Equal(entity.Iron, result.Iron);
        Assert.Equal(entity.Nickel, result.Nickel);

        repositoryMock.Verify(repository => repository.GetByIdAsync(42), Times.Once);
    }

    private static CelestialObject CreateEntity(long objectId, string objectName)
    {
        return new CelestialObject
        {
            ObjectId = objectId,
            ObjectName = objectName,
            Category = "Planet",
            DistanceLightYears = 1.2m,
            DiscoveryDate = new DateTime(2024, 1, 1),
            InSolarSystem = false,
            HabitabilityScore = 7.5m,
            SurfaceTemperature = 12.3m,
            Gravity = 9.8m,
            Nitrogen = true,
            Oxygen = true,
            Co2 = false,
            SulfuricAcid = false,
            Hydrogen = false,
            Helium = false,
            Methane = false,
            WaterVapor = true,
            Silicates = true,
            Iron = true,
            Nickel = false
        };
    }

    [Fact]
    public async Task CreateAsync_ReturnsSuccess_WhenRequestIsValidAndIdIsUnique()
    {
        var dto = CreateCreateDto(objectId: 1200, objectName: "Planet X");
        CelestialObject? addedEntity = null;

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.ExistsAsync(dto.ObjectId))
            .ReturnsAsync(false);
        repositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<CelestialObject>()))
            .Callback<CelestialObject>(entity => addedEntity = entity)
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(repository => repository.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.CreateAsync(dto);

        Assert.Equal(CelestialObjectMutationStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.ObjectId, result.Data!.ObjectId);
        Assert.NotNull(addedEntity);
        Assert.Equal(dto.ObjectName, addedEntity!.ObjectName);
        Assert.Equal(dto.Category, addedEntity.Category);
        Assert.True(addedEntity.InSolarSystem);
        Assert.True(addedEntity.Oxygen);
        Assert.False(addedEntity.Co2);

        repositoryMock.Verify(repository => repository.ExistsAsync(dto.ObjectId), Times.Once);
        repositoryMock.Verify(repository => repository.AddAsync(It.IsAny<CelestialObject>()), Times.Once);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDuplicate_WhenIdAlreadyExists()
    {
        var dto = CreateCreateDto(objectId: 5, objectName: "Duplicate");

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.ExistsAsync(dto.ObjectId))
            .ReturnsAsync(true);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.CreateAsync(dto);

        Assert.Equal(CelestialObjectMutationStatus.Duplicate, result.Status);
        repositoryMock.Verify(repository => repository.AddAsync(It.IsAny<CelestialObject>()), Times.Never);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsSuccess_WhenObjectExists()
    {
        var entity = CreateEntity(objectId: 2000, objectName: "Old Name");
        var dto = CreateUpdateDto(objectName: "New Name");
        CelestialObject? updatedEntity = null;

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(2000))
            .ReturnsAsync(entity);
        repositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<CelestialObject>()))
            .Callback<CelestialObject>(updated => updatedEntity = updated)
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(repository => repository.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.UpdateAsync(2000, dto);

        Assert.Equal(CelestialObjectMutationStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal("New Name", result.Data!.ObjectName);
        Assert.NotNull(updatedEntity);
        Assert.Equal("New Name", updatedEntity!.ObjectName);
        Assert.True(updatedEntity.Nitrogen);
        Assert.False(updatedEntity.Oxygen);

        repositoryMock.Verify(repository => repository.GetByIdAsync(2000), Times.Once);
        repositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<CelestialObject>()), Times.Once);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_WhenObjectMissing()
    {
        var dto = CreateUpdateDto(objectName: "Missing");

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(4040))
            .ReturnsAsync((CelestialObject?)null);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.UpdateAsync(4040, dto);

        Assert.Equal(CelestialObjectMutationStatus.NotFound, result.Status);
        repositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<CelestialObject>()), Times.Never);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsSuccess_WhenObjectExists()
    {
        var entity = CreateEntity(objectId: 3000, objectName: "Delete Me");

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(3000))
            .ReturnsAsync(entity);
        repositoryMock
            .Setup(repository => repository.DeleteAsync(entity))
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(repository => repository.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.DeleteAsync(3000);

        Assert.Equal(CelestialObjectMutationStatus.Success, result.Status);
        repositoryMock.Verify(repository => repository.DeleteAsync(entity), Times.Once);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFound_WhenObjectMissing()
    {
        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(3001))
            .ReturnsAsync((CelestialObject?)null);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.DeleteAsync(3001);

        Assert.Equal(CelestialObjectMutationStatus.NotFound, result.Status);
        repositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<CelestialObject>()), Times.Never);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_MapsRequestDtoToEntityFields()
    {
        var dto = new CreateCelestialObjectDto
        {
            ObjectId = 4010,
            ObjectName = "Mapper",
            Category = "Exoplanet",
            DistanceLightYears = 15.123456m,
            DiscoveryDate = new DateTime(2025, 2, 1),
            InSolarSystem = "N",
            HabitabilityScore = 8.5m,
            SurfaceTemperature = -45.20m,
            Gravity = 0.95m,
            Nitrogen = "Y",
            Oxygen = "Y",
            Co2 = "N",
            SulfuricAcid = "N",
            Hydrogen = "Y",
            Helium = "N",
            Methane = "Y",
            WaterVapor = "Y",
            Silicates = "N",
            Iron = "Y",
            Nickel = "N"
        };

        CelestialObject? addedEntity = null;

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.ExistsAsync(dto.ObjectId))
            .ReturnsAsync(false);
        repositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<CelestialObject>()))
            .Callback<CelestialObject>(entity => addedEntity = entity)
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(repository => repository.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        await service.CreateAsync(dto);

        Assert.NotNull(addedEntity);
        Assert.Equal(dto.ObjectId, addedEntity!.ObjectId);
        Assert.Equal(dto.ObjectName, addedEntity.ObjectName);
        Assert.Equal(dto.Category, addedEntity.Category);
        Assert.Equal(dto.DistanceLightYears, addedEntity.DistanceLightYears);
        Assert.Equal(dto.DiscoveryDate, addedEntity.DiscoveryDate);
        Assert.False(addedEntity.InSolarSystem);
        Assert.True(addedEntity.Nitrogen);
        Assert.True(addedEntity.Oxygen);
        Assert.False(addedEntity.Co2);
        Assert.True(addedEntity.Hydrogen);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsMappedResponseDtoAfterUpdate()
    {
        var entity = CreateEntity(objectId: 5001, objectName: "Before");
        var dto = CreateUpdateDto(objectName: "After");

        var repositoryMock = new Mock<ICelestialObjectRepository>();
        repositoryMock
            .Setup(repository => repository.GetByIdAsync(5001))
            .ReturnsAsync(entity);
        repositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<CelestialObject>()))
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(repository => repository.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = new CelestialObjectService(repositoryMock.Object, NullLogger<CelestialObjectService>.Instance);

        var result = await service.UpdateAsync(5001, dto);

        Assert.Equal(CelestialObjectMutationStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal(5001, result.Data!.ObjectId);
        Assert.Equal("After", result.Data.ObjectName);
        Assert.Equal(dto.Category, result.Data.Category);
        Assert.Equal(dto.Gravity, result.Data.Gravity);
    }

    private static CreateCelestialObjectDto CreateCreateDto(long objectId, string objectName)
    {
        return new CreateCelestialObjectDto
        {
            ObjectId = objectId,
            ObjectName = objectName,
            Category = "Planet",
            DistanceLightYears = 3.50m,
            DiscoveryDate = new DateTime(2024, 1, 1),
            InSolarSystem = "Y",
            HabitabilityScore = 7.8m,
            SurfaceTemperature = 23.4m,
            Gravity = 9.8m,
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

    private static UpdateCelestialObjectDto CreateUpdateDto(string objectName)
    {
        return new UpdateCelestialObjectDto
        {
            ObjectName = objectName,
            Category = "Exoplanet",
            DistanceLightYears = 9.999999m,
            DiscoveryDate = new DateTime(2023, 5, 20),
            InSolarSystem = "N",
            HabitabilityScore = 6.6m,
            SurfaceTemperature = -11.4m,
            Gravity = 4.5m,
            Nitrogen = "Y",
            Oxygen = "N",
            Co2 = "Y",
            SulfuricAcid = "N",
            Hydrogen = "Y",
            Helium = "Y",
            Methane = "N",
            WaterVapor = "N",
            Silicates = "Y",
            Iron = "N",
            Nickel = "Y"
        };
    }
}
