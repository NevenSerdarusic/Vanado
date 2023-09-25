
using EquipmentManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EquipmentManagement.API.Test;


public class MachineControllerTests
{
    private readonly MachinesController _machinesController;

    //DI
    public MachineControllerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json") 
            .Build();

        // IConfiguration injection
        _machinesController = new MachinesController(configuration);
    }

    [Fact]
    public void Test_AddMachine_ValidData()
    {
        //ARRANGE
        var controller = _machinesController;

        var validMachine = new Machine
        {
            Name = "Pokretna traka"
        };

        //ACT
        var result = controller.AddMachine(validMachine) as OkObjectResult;

        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var adddedMAchine = result.Value as Machine;
        Assert.NotNull(adddedMAchine);
    }

    [Fact]
    public void Test_UpdateMachine_ValidData()
    {
        //ARRANGE
        var controller = _machinesController;

        var machine = new Machine
        {
            Id = 1,
            Name = "Tunelska peć"
        };

        //ACT
        var result = controller.UpdateMachine(machine.Id, machine) as OkObjectResult;


        //ASSERT
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var updatedMachine = result.Value as Machine;
        Assert.NotNull(updatedMachine);
        Assert.Equal(machine.Id, updatedMachine.Id);
        Assert.Equal(machine.Name, updatedMachine.Name);

    }



}
