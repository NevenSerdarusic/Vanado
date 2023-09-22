using EquipmentManagement.API.Models;
using EquipmentManagement.API.Repositories;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class MachinesController : ControllerBase
{
    private readonly MachineRepository _machineRepository;

    private readonly FailureRepository _failureRepository;

    public MachinesController(IConfiguration configuration)
    {
        _machineRepository = new MachineRepository(configuration.GetConnectionString("DefaultConnection"));
        _failureRepository = new FailureRepository(configuration.GetConnectionString("DefaultConnection"));
    }

    // GET api/machines
    [HttpGet]
    public IActionResult GetMachines()
    {
        var machines = _machineRepository.GetAllMachines();
        return Ok(machines);
    }

    // GET api/machines/{id}
    [HttpGet("{id}")]
    public IActionResult GetMachine(int id)
    {
        var machine = _machineRepository.GetMachineById(id);
        if (machine == null)
        {
            return NotFound();
        }
        return Ok(machine);
    }

    // POST api/machines
    [HttpPost]
    public IActionResult AddMachine([FromBody] Machine machine)
    {
        if (machine == null)
        {
            return BadRequest();
        }

        var addedMachine = _machineRepository.AddMachine(machine);

        if (addedMachine == null)
        {
            return Conflict("Equipment with same name you trying to add is already exists in database.");
        }

        return Ok(addedMachine);
    }

    // PUT api/machines/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateMachine(int id, [FromBody] Machine machine)
    {
        if (machine == null || id != machine.Id)
        {
            return BadRequest();
        }

        var updated = _machineRepository.UpdateMachine(machine);

        if (updated)
        {
            return Ok(machine);
        }
        else
        {
            return StatusCode(500, "Failed to update equipment.");
        }
    }

    // DELETE api/machines/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteMachine(int id)
    {
        var deleted = _machineRepository.DeleteMachine(id);

        if (deleted)
        {
            return NoContent();
        }
        else
        {
            return StatusCode(500, "Failed to delete equipment.");
        }
    }


    // GET api/machines/{machineId}/details
    [HttpGet("{machineId}/details")]
    public IActionResult GetMachineDetails(int machineId)
    {
        var machine = _machineRepository.GetMachineById(machineId);
        if (machine == null)
        {
            return NotFound("Equipment not found.");
        }

        var failures = _failureRepository.GetFailuresByMachineId(machineId);

        // Calculation of the average duration of failures on a specific machine
        double totalDurationHours = 0;

        foreach (var failure in failures)
        {
            if (failure.EndTime.HasValue && failure.StartTime != DateTime.MinValue)
            {
                totalDurationHours += (failure.EndTime - failure.StartTime)?.TotalHours ?? 0;
            }
        }

        double averageDurationHours = failures.Any() ? totalDurationHours / failures.Count() : 0;

        // Converting to hours, minutes and seconds
        int averageDurationHoursInt = (int)averageDurationHours;
        int averageDurationMinutes = (int)((averageDurationHours - averageDurationHoursInt) * 60);
        int averageDurationSeconds = (int)(((averageDurationHours - averageDurationHoursInt) * 60 - averageDurationMinutes) * 60);

        var machineDetails = new
        {
            MachineName = machine.Name,
            Failures = failures,
            AverageDuration = new
            {
                Hours = averageDurationHoursInt,
                Minutes = averageDurationMinutes,
                Seconds = averageDurationSeconds
            }
        };


        return Ok(machineDetails);
    }

}
