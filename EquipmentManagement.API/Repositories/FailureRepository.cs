using Dapper;
using EquipmentManagement.API.Models;
using Npgsql;

namespace EquipmentManagement.API.Repositories;

public class FailureRepository
{
    private readonly string _connectionString;

    public FailureRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    #region //CONTROLLER ACTION --> GetFailures
    public IEnumerable<Failure> GetAllFailures()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            var query = @"
            SELECT f.*, m.Name AS MachineName
            FROM Failures f
            LEFT JOIN Machines m ON f.MachineId = m.Id
        ";
            var failures = connection.Query<Failure, string, Failure>(
            query,
            (failure, machineName) =>
            {
                if (failure.Machine == null)
                {
                    failure.Machine = new Machine();
                }
                failure.Machine.Name = machineName;
                return failure;
            },
            splitOn: "MachineName"
        );
            return failures;
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> GetFailure + UpdateFailureStatus
    public Failure GetFailureById(int id)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            var query = @"
            SELECT f.*, m.Name AS MachineName
            FROM Failures f
            LEFT JOIN Machines m ON f.MachineId = m.Id
            WHERE f.Id = @Id
        ";

            var failure = connection.Query<Failure, string, Failure>(
                query,
                (failure, machineName) =>
                {
                    if (failure.Machine == null)
                    {
                        failure.Machine = new Machine();
                    }
                    failure.Machine.Name = machineName;
                    return failure;
                },
                new { Id = id },
                splitOn: "MachineName"
            ).FirstOrDefault();

            return failure;
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> AddFailure
    public Failure GetActiveFailureByMachineId(int machineId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            var query = @"
                    SELECT *
                    FROM Failures
                    WHERE MachineId = @MachineId
                      AND IsResolved = false
                ";

            return connection.QueryFirstOrDefault<Failure>(query, new { MachineId = machineId });
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> AddFailure
    public Failure AddFailure(Failure failure)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            
            var query = "INSERT INTO Failures (Name, MachineId, Priority, StartTime, Description) VALUES (@Name, @MachineId, @Priority, @StartTime, @Description) RETURNING Id";
            var insertedId = connection.ExecuteScalar<int>(query, failure);
            failure.Id = insertedId;

            return failure;
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> UpdateFailure
    public bool UpdateFailure(Failure failure)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var query = "UPDATE Failures SET Name = @Name, MachineId = @MachineId, Priority = @Priority, StartTime = @StartTime, EndTime = @EndTime, Description = @Description, IsResolved = @IsResolved WHERE Id = @Id";
            var affectedRows = connection.Execute(query, failure);
            return affectedRows > 0;
        }
    }
    #endregion

    #region //CONTROLLER: MachinesController ; CONTROLLER ACTION --> GetMachinesDetails
    public IEnumerable<Failure> GetFailuresByMachineId(int machineId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            var query = @"
            SELECT *
            FROM Failures
            WHERE MachineId = @MachineId
        ";

            var failures = connection.Query<Failure>(query, new { MachineId = machineId });
            return failures;
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> DeleteFailure
    public bool DeleteFailure(int id)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var query = "DELETE FROM Failures WHERE Id = @Id";
            var affectedRows = connection.Execute(query, new { Id = id });
            return affectedRows > 0;
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> GetSortedFailures
    public IEnumerable<Failure> GetSortedFailures(int startIndex, int pageSize)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            var query = @"
            SELECT *
            FROM Failures
            ORDER BY 
                CASE Priority
                    WHEN 3 THEN 1
                    WHEN 2 THEN 2
                    WHEN 1 THEN 3
                    ELSE 4
                END,
                StartTime ASC
            OFFSET @StartIndex ROWS FETCH NEXT @PageSize ROWS ONLY
        ";

            var sortedFailures = connection.Query<Failure>(query, new { StartIndex = startIndex, PageSize = pageSize });
            return sortedFailures;
        }
    }
    #endregion

    #region //CONTROLLER ACTION --> UpdateFailureStatus
    public bool UpdateFailureStatus(Failure failure)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            var query = "UPDATE Failures SET IsResolved = @IsResolved, EndTime = @EndTime WHERE Id = @Id";
            var affectedRows = connection.Execute(query, failure);

            return affectedRows > 0;
        }
    }
    #endregion




}

