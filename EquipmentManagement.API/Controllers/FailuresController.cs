using EquipmentManagement.API.Models;
using EquipmentManagement.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FailuresController : ControllerBase
{
    private readonly FailureRepository _failureRepository;
    private readonly MachineRepository _machineRepository;

    public FailuresController(IConfiguration configuration)
    {
        _failureRepository = new FailureRepository(configuration.GetConnectionString("DefaultConnection"));
        _machineRepository = new MachineRepository(configuration.GetConnectionString("DefaultConnection"));
    }

    // GET api/failures
    [HttpGet]
    public IActionResult GetFailures()
    {
        var failures = _failureRepository.GetAllFailures();
        return Ok(failures);
    }

    // GET api/failures/{id}
    [HttpGet("{id}")]
    public IActionResult GetFailure(int id)
    {
        var failure = _failureRepository.GetFailureById(id);
        if (failure == null)
        {
            return NotFound();
        }
        return Ok(failure);
    }

    // POST api/failures
    [HttpPost]
    public IActionResult AddFailure([FromBody] Failure failure)
    {
        if (failure == null)
        {
            return BadRequest();
        }

        //Check if there is a machine with a matching MachineId
        var machine = _machineRepository.GetMachineById(failure.MachineId);
        if (machine == null) 
        {
            return BadRequest(new { message = "Machine with the specified MachineId does not exist." });
        }


        //Check if there is an active failure on the same machine, do not allow saving a new failure
        var activeFailure = _failureRepository.GetActiveFailureByMachineId(failure.MachineId);

        if (activeFailure != null)
        {
            return BadRequest(new { message = "There is an active failure on the same equipment, you cannot report a new failure until you resolve first failure" });
        }

        //Check if the string remains default in Failure Name, if it is warn that the name must be added
        if (failure.Name == "string")
        {
            return BadRequest(new { message = "Failure name cant be default string, you need to write failure name to the related equipment" });
        }

        //Check if the string remains default in Failure Description, if it is warn that the description must be added
        if (failure.Description == "string")
        {
            return BadRequest(new { message = "Failure description cant be default string, you need to write failure description to the related equipment" });
        }

        //Check if the status is set to true, if it is warn that the status must be set to false
        if (failure.IsResolved)
        {
            return BadRequest(new { message = "If you are reporting a failure on equipment, the status should be false." });
        }

        var addedFailure = _failureRepository.AddFailure(failure);

        return Ok(addedFailure);
    }

    // PUT api/failures/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateFailure(int id, [FromBody] Failure updatedFailure)
    {
        if (updatedFailure == null || id != updatedFailure.Id)
        {
            return BadRequest();
        }

        // Retrieve the existing failure from the repository
        var existingFailure = _failureRepository.GetFailureById(id);

        if (existingFailure == null)
        {
            return NotFound("Failure not found.");
        }

        // Check if the machineId is being changed
        if (existingFailure.MachineId != updatedFailure.MachineId)
        {
            // Check if there is an active failure on the new machine, do not allow the change
            var activeFailureOnNewMachine = _failureRepository.GetActiveFailureByMachineId(updatedFailure.MachineId);

            if (activeFailureOnNewMachine != null)
            {
                return BadRequest(new { message = "There is an active failure on the new equipment, you cannot update the failure until you resolve the active failure on the new equipment." });
            }
        }

        // Check if the provided EndTime is valid and greater than or equal to StartTime
        if (updatedFailure.EndTime.HasValue && updatedFailure.EndTime < updatedFailure.StartTime)
        {
            return BadRequest(new { message = "EndTime must be greater than or equal to StartTime." });
        }

        // Automatically set isResolved to true if a valid EndTime is provided
        if (updatedFailure.EndTime.HasValue)
        {
            updatedFailure.IsResolved = true;
        }


        // Update other properties of the failure as needed.
        existingFailure.Name = updatedFailure.Name;
        existingFailure.MachineId = updatedFailure.MachineId;
        existingFailure.Priority = updatedFailure.Priority;
        existingFailure.StartTime = updatedFailure.StartTime;
        existingFailure.EndTime = updatedFailure.EndTime;
        existingFailure.Description = updatedFailure.Description;
        existingFailure.IsResolved = updatedFailure.IsResolved;

        // Check if the status is changing from false to true, update EndTime to the current time.
        if (!existingFailure.IsResolved && updatedFailure.IsResolved)
        {
            existingFailure.EndTime = DateTime.Now;
        }
        // Check if the status is changing from true to false, set EndTime to null.
        else if (existingFailure.IsResolved && !updatedFailure.IsResolved)
        {
            existingFailure.EndTime = null;
        }


        var updated = _failureRepository.UpdateFailure(existingFailure);

        if (updated)
        {
            return Ok(existingFailure);
        }
        else
        {
            return StatusCode(500, "Failed to update failure.");
        }
    }


    // DELETE api/failures/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteFailure(int id)
    {
        var deleted = _failureRepository.DeleteFailure(id);

        if (deleted)
        {
            return NoContent();
        }
        else
        {
            return StatusCode(500, "Failed to delete failure.");
        }
    }


    // GET api/failures/sorted
    [HttpGet("sorted")]
    public IActionResult GetSortedFailures(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // Pagination validation
        if (page < 1)
        {
            return BadRequest("Page must be a positive integer.");
        }

        //Calculate the starting index for data pagination.
        int startIndex = (page - 1) * pageSize;

        //Get failures from respository
        var sortedFailures = _failureRepository.GetSortedFailures(startIndex, pageSize);

        if (sortedFailures == null || !sortedFailures.Any())
        {
            return NotFound();
        }

        return Ok(sortedFailures);
    }


    [HttpPut("{id}/status")]
    public IActionResult UpdateFailureStatus(int id, [FromBody] bool isResolved)
    {
        //Retrieve the failure by its ID from the repository
        var failure = _failureRepository.GetFailureById(id);

        //Check if the failure exists.
        if (failure == null)
        {
            return NotFound("Failure not found.");
        }

        // If the status is changing from false to true, update EndTime to the current time.
        if (!failure.IsResolved && isResolved)
        {
            failure.EndTime = DateTime.Now;
        }

        // If the status is changing from true to false, set EndTime to null.
        if (failure.IsResolved && !isResolved)
        {
            failure.EndTime = null;
        }

        //Update the IsResolved property of the failure with the provided isResolved value.
        failure.IsResolved = isResolved;

        //Update the failure's status in the repository.
        var success = _failureRepository.UpdateFailureStatus(failure);

        if (!success)
        {
            return StatusCode(500, "Failed to update failure status.");
        }

        return Ok(failure);
    }


}
