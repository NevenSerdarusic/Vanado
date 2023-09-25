

using EquipmentManagement.API.Controllers;
using EquipmentManagement.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EquipmentManagement.API.Test;

public class FailureControllerTests
{
    private readonly FailuresController _failuresController;


    //DI
    public FailureControllerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // IConfiguration injection
        _failuresController = new FailuresController(configuration);
    }

    [Fact]
    public void Test_AddFailure_InvalidMachineId()
    {
        //ARRANGE
        var controller = _failuresController;

        var invalidFailure = new Failure
        {
            Name = "Blokiran udarni mehanizam",
            Id = 102,
            Priority = (Helper.Priority)2,
            StartTime = DateTime.Now,
            Description = "Udarni mehanizam sa lijeve strane silosa ne radi",
            IsResolved = false
        };

        //ACT
        var result = controller.AddFailure(invalidFailure) as BadRequestObjectResult;

        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        var message = result.Value as IDictionary<string, string>;
        Assert.NotNull(message);
        Assert.True(message.ContainsKey("message"));
        Assert.Equal("Machine with the specified MachineId does not exist.", message["message"]);
    }

    [Fact]
    public void Test_GetFailureById_WithExistingId()
    {
        //ARRANGE
        var controller = _failuresController;
        var existingId = 3;

        //ACT
        var result = controller.GetFailure(existingId) as OkObjectResult;

        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        var failure = result.Value as Failure;
        Assert.NotNull(failure);
    }
}
