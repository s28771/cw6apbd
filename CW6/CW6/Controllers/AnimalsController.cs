using CW6.Models;
using CW6.Models.DTDs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CW6.Controllers;

[ApiController]
//[Route("/animals")]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpGet]
    public IActionResult GetAnimals()
    {
        using (var connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            connection.Open();
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandText = "SELECT IdAnimal, Name, Description, Category, Area FROM Animal;";

            var reader = command.ExecuteReader();

            List<Animal> animals = new List<Animal>();
            int idAnimalOrdinal = reader.GetOrdinal("IdAnimal");
            int nameOrdinal = reader.GetOrdinal("Name");
            int descriptionOrdinal = reader.GetOrdinal("Description");
            int categoryOrdinal = reader.GetOrdinal("Category");
            int areaOrdinal = reader.GetOrdinal("Area");
        
            while (reader.Read())
            {
                animals.Add(new Animal()
                {
                    IdAnimal = reader.GetInt32(idAnimalOrdinal),
                    Name = reader.GetString(nameOrdinal),
                    Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                    Category = reader.IsDBNull(categoryOrdinal) ? null : reader.GetString(categoryOrdinal),
                    Area = reader.IsDBNull(areaOrdinal) ? null : reader.GetString(areaOrdinal)
                });
            }
            return Ok(animals);
        }
    }


    [HttpPost]
    public IActionResult PostAnimal([FromBody] AddAnimal model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        using (var connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            connection.Open();
            var query = @"
            INSERT INTO Animal (Name, Description, Category, Area) 
            VALUES (@Name, @Description, @Category, @Area); 
            SELECT SCOPE_IDENTITY();";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", model.Name);
                command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(model.Description) ? DBNull.Value : model.Description);
                command.Parameters.AddWithValue("@Category", string.IsNullOrEmpty(model.Category) ? DBNull.Value : model.Category);
                command.Parameters.AddWithValue("@Area", string.IsNullOrEmpty(model.Area) ? DBNull.Value : model.Area);

                var id = Convert.ToInt32(command.ExecuteScalar());

                if (id > 0)
                {
                    var animal = new Animal
                    {
                        IdAnimal = id,
                        Name = model.Name,
                        Description = model.Description,
                        Category = model.Category,
                        Area = model.Area
                    };
                    return CreatedAtAction("GetAnimals", new { id = id }, animal);
                }
                else
                {
                    return BadRequest("Failed to create animal");
                }
            }
        }
    }

    [HttpPut("{id}")]
    public IActionResult PutAnimal(int id, [FromBody] Animal animal)
    {
        using (var connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            connection.Open();
            var query = @"
            UPDATE Animal 
            SET Name = @Name, Description = @Description, Category = @Category, Area = @Area 
            WHERE IdAnimal = @Id;";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Description", animal.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Category", animal.Category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Area", animal.Area ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Id", id);

                var result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
        }
    }
    [HttpDelete("{id}")]
    public IActionResult DeleteAnimal(int id)
    {
        using (var connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            connection.Open();
            var query = "DELETE FROM Animal WHERE IdAnimal = @Id;";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
        }
    }
}