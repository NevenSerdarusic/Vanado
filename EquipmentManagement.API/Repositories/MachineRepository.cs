using Dapper;
using EquipmentManagement.API.Models;
using Npgsql;

namespace EquipmentManagement.API.Repositories;

public class MachineRepository
{
    private readonly string _connectionString;

    public MachineRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    #region //CONTROLLER ACTION --> GetMachines
    public IEnumerable<Machine> GetAllMachines()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.Query<Machine>("SELECT * FROM Machines");
        }
    }
    #endregion

    #region //CONTROLLER: MachineController CONTROLLER ACTION --> GetMachine + CONTROLLER: FailuresController CONTROLLER ACTION --> AddFailure
    public Machine GetMachineById(int id)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            return connection.QueryFirstOrDefault<Machine>("SELECT * FROM Machines WHERE Id = @Id", new { Id = id });
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> AddMachine
    public Machine AddMachine(Machine machine)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            // CHECKING IF THERE IS ALREADY EQUIPMENT WITH SAME NAME IN DATABASE
            var existingMachine = connection.QueryFirstOrDefault<Machine>("SELECT * FROM Machines WHERE Name = @Name", new { Name = machine.Name });

            if (existingMachine != null)
            {
                return null; 
            }

            // ADDING NEW EQUIPMENT TO THE DATABASE
            var query = "INSERT INTO Machines (Name) VALUES (@Name) RETURNING Id";
            var insertedId = connection.ExecuteScalar<int>(query, machine);
            machine.Id = insertedId;

            return machine;
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> UpdateMachine
    public bool UpdateMachine(Machine machine)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var query = "UPDATE Machines SET Name = @Name WHERE Id = @Id";
            var affectedRows = connection.Execute(query, machine);
            return affectedRows > 0;
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> DeleteMachine
    public bool DeleteMachine(int id)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var query = "DELETE FROM Machines WHERE Id = @Id";
            var affectedRows = connection.Execute(query, new { Id = id });
            return affectedRows > 0;
        }
    }
    #endregion
}

