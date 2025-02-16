using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework con SQLite
builder.Services.AddDbContext<QuestionnaireContext>(options =>
    options.UseSqlite("Data Source=questionnaire.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuestionnaireContext>();
    db.Database.EnsureCreated();
}

ApiEndpoints.RegisterEndpoints(app);

app.Run();

// ============================
//  Modelos de Datos
// ============================
public class QuestionnaireContext : DbContext
{
    public QuestionnaireContext(DbContextOptions<QuestionnaireContext> options) : base(options) { }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Response> Responses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configura la herencia TPH para las clases derivadas de Question
        modelBuilder.Entity<Question>()
            .HasDiscriminator<string>("QuestionType")
            .HasValue<StarRatingQuestion>("StarRatingQuestion")
            .HasValue<MultiSelectQuestion>("MultiSelectQuestion")
            .HasValue<SingleSelectQuestion>("SingleSelectQuestion");
    }
}

public abstract class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class StarRatingQuestion : Question
{
    public int MinValue { get; set; } = 1;
    public int MaxValue { get; set; } = 5;
}

public class MultiSelectQuestion : Question
{
    public List<string> Options { get; set; } = new();
}

public class SingleSelectQuestion : Question
{
    public List<string> Options { get; set; } = new();
}

public class Response
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
}

// ============================
// Endpoints
// ============================
public static class ApiEndpoints
{
    public static void RegisterEndpoints(WebApplication app)
    {
        // Endpoint para crear preguntas (PUT)
        app.MapPut("/question/{id:guid}", async ([FromRoute] Guid id, [FromBody] JsonElement questionJson, QuestionnaireContext db) =>
        {
            // Intentar deserializar según el tipo específico de la pregunta
            Question question = null;
            var type = questionJson.GetProperty("Type").GetString();

            if (type == "SingleSelectQuestion")
            {
                question = JsonSerializer.Deserialize<SingleSelectQuestion>(questionJson.ToString());
            }
            else if (type == "StarRatingQuestion")
            {
                question = JsonSerializer.Deserialize<StarRatingQuestion>(questionJson.ToString());
            }
            else if (type == "MultiSelectQuestion")
            {
                question = JsonSerializer.Deserialize<MultiSelectQuestion>(questionJson.ToString());
            }

            if (question == null)
            {
                return Results.BadRequest("Tipo de pregunta no válido");
            }

            question.Id = id;
            db.Questions.Add(question);
            await db.SaveChangesAsync();
            return Results.Created($"/question/{id}", question);
        });

        // Endpoint para responder preguntas (POST)
        app.MapPost("/response", async ([FromBody] Response response, QuestionnaireContext db) =>
        {
            var question = await db.Questions.FindAsync(response.QuestionId);
            if (question == null)
                return Results.NotFound("Pregunta no encontrada");

            // Validar la respuesta según el tipo de pregunta
            switch (question)
            {
                case StarRatingQuestion starQuestion:
                    if (!int.TryParse(response.Answer, out int rating) || rating < starQuestion.MinValue || rating > starQuestion.MaxValue)
                        return Results.BadRequest($"La respuesta debe ser un número entre {starQuestion.MinValue} y {starQuestion.MaxValue}");
                    break;

                case SingleSelectQuestion singleSelectQuestion:
                    if (!singleSelectQuestion.Options.Contains(response.Answer))
                        return Results.BadRequest("La respuesta debe ser una de las opciones disponibles");
                    break;

                case MultiSelectQuestion multiSelectQuestion:
                    var selectedOptions = response.Answer.Split(",").Select(o => o.Trim()).ToList();
                    if (!selectedOptions.All(opt => multiSelectQuestion.Options.Contains(opt)))
                        return Results.BadRequest("Una o más respuestas no están en las opciones disponibles");
                    break;
            }

            response.QuestionId = question.Id;
            db.Responses.Add(response);
            await db.SaveChangesAsync();
            return Results.Ok(response);
        });
    }
}