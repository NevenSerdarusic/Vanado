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


    public Failure AddFailure(Failure failure)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            // Dodajte kvar u bazu podataka
            var query = "INSERT INTO Failures (Name, MachineId, Priority, StartTime, Description) VALUES (@Name, @MachineId, @Priority, @StartTime, @Description) RETURNING Id";
            var insertedId = connection.ExecuteScalar<int>(query, failure);
            failure.Id = insertedId;

            return failure;
        }
    }

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

    public IEnumerable<Failure> GetSortedFailures(int startIndex, int pageSize)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            var query = @"
            SELECT *
            FROM Failures
            ORDER BY Priority ASC, StartTime DESC
            OFFSET @StartIndex ROWS FETCH NEXT @PageSize ROWS ONLY
        ";

            var sortedFailures = connection.Query<Failure>(query, new { StartIndex = startIndex, PageSize = pageSize });
            return sortedFailures;
        }
    }


}

