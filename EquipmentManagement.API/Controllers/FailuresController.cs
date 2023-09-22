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

    public FailuresController(IConfiguration configuration)
    {
        _failureRepository = new FailureRepository(configuration.GetConnectionString("DefaultConnection"));
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

        var activeFailure = _failureRepository.GetActiveFailureByMachineId(failure.MachineId);

        //If there is an active failure on the same machine, do not allow saving a new failure
        if (activeFailure != null)
        {
            return BadRequest(new { message = "There is an active failure on the same machine, you cannot report a new failure until you resolve first failure" });
        }

        // If the string remains default, it warns that the description must be added
        if (failure.Description == "string")
        {
            return BadRequest(new { message = "Failure description is missing, you need to write failure related to equipment" });
        }

        var addedFailure = _failureRepository.AddFailure(failure);

        return Ok(addedFailure);
    }

    // PUT api/failures/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateFailure(int id, [FromBody] Failure failure)
    {
        if (failure == null || id != failure.Id)
        {
            return BadRequest();
        }

        var updated = _failureRepository.UpdateFailure(failure);

        if (updated)
        {
            return Ok(failure);
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
