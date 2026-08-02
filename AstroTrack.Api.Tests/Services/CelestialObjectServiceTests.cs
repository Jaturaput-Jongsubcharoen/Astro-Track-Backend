using AstroTrack.Api.DTOs.CelestialObjects;
using AstroTrack.Api.Models;
using AstroTrack.Api.Repositories;
using AstroTrack.Api.Services;
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

        var service = new CelestialObjectService(repositoryMock.Object);

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

        var service = new CelestialObjectService(repositoryMock.Object);

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

        var service = new CelestialObjectService(repositoryMock.Object);

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

        var service = new CelestialObjectService(repositoryMock.Object);

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

        var service = new CelestialObjectService(repositoryMock.Object);

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
}
