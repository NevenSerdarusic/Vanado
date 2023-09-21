using Npgsql;
using Dapper;
using System.Reflection.PortableExecutable;
using EquipmentManagement.API.Models;
using EquipmentManagement.API.Helper;
using EquipmentManagement.API.Controllers;

namespace EquipmentManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("appsettings.json");

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");



            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Create Machines Table
                string createMachinesTableSql = @"
                CREATE TABLE IF NOT EXISTS Machines (
                    Id serial PRIMARY KEY,
                    Name VARCHAR(50) NOT NULL
                )";

                
                connection.Execute(createMachinesTableSql);

                // Create Failures Table
                string createFailuresTableSql = @"
                CREATE TABLE IF NOT EXISTS Failures (
                    Id serial PRIMARY KEY,
                    Name VARCHAR(50) NOT NULL,
                    MachineId INT NOT NULL,
                    Priority INT NOT NULL,
                    StartTime TIMESTAMP NOT NULL,
                    EndTime TIMESTAMP,
                    Description TEXT NOT NULL,
                    IsResolved BOOLEAN DEFAULT FALSE
                )";

                
                connection.Execute(createFailuresTableSql);


                connection.Close();
            }

            builder.Services.AddControllers();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();


            app.Run();
        }
    }
}