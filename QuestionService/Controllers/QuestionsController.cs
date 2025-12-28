using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionsController(QuestionDbContext db): ControllerBase
{
    public async Task<ActionResult<Question>> CreateQuestion(CreateQuestionDto dto)
    {
        var validTags = await db.Tags.Where(x => dto.Tags.Contains(x.Slug)).ToListAsync();
        
        var missing = dto.Tags.Except(validTags.Select(x => x.Slug)).ToList().ToList();
        
        if (missing.Count != 0)
            return BadRequest($"Invalid tags: {string.Join(", ", missing)}");
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue("name");
        
        if (userId is null || name is null) return BadRequest("cannot get user details");
        
        var question = new Question
        {
            Title = dto.Title,
            Content = dto.Content,
            TagSlugs = dto.Tags,
            AskerId = userId
        };
        
        try
        {
            db.Questions.Add(question);
            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return Created($"questions/{question.Id}", question);
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Question>>> GetQuestions(string? tag)
    {
        var query = db.Questions.AsQueryable();

        if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(x => x.TagSlugs.Contains(tag));
        }

        var result = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();

        return result;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Question>> GetQuestion(string id)
    {
        var question = await db.Questions.FindAsync(id);
    
        if (question is null) return NotFound();

        await db.Questions
            .Where(q => q.Id == id)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(q => q.ViewCount, q => q.ViewCount + 1));

        return question;
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateQuestion(string id, CreateQuestionDto dto)
    {
        var question = await db.Questions.FindAsync(id);
        if (question is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != question.AskerId)
            return Forbid();
        
        var validTags = await db.Tags.Where(x => dto.Tags.Contains(x.Slug)).ToListAsync();
        
        var missing = dto.Tags.Except(validTags.Select(x => x.Slug)).ToList().ToList();
        
        if (missing.Count != 0)
            return BadRequest($"Invalid tags: {string.Join(", ", missing)}");
        
        // if (!await tagService.AreTagsValidAsync(dto.Tags))
        //     return BadRequest("Invalid tags");
        //
        // var original = question.TagSlugs
        //     .Distinct(StringComparer.OrdinalIgnoreCase)
        //     .ToArray();
        //
        // var incoming = dto.Tags
        //     .Distinct(StringComparer.OrdinalIgnoreCase)
        //     .ToArray();
        //
        // var removed = original.Except(incoming, StringComparer.OrdinalIgnoreCase).ToArray();
        // var added   = incoming.Except(original, StringComparer.OrdinalIgnoreCase).ToArray();
        //
        // var sanitizer = new HtmlSanitizer();

        question.Title = dto.Title;
        question.Content = dto.Content;
        question.TagSlugs = dto.Tags;
        question.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return NoContent();
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteQuestion(string id)
    {
        var question = await db.Questions.FindAsync(id);
        if (question is null) return NotFound();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != question.AskerId)
            return Forbid();
        
        db.Questions.Remove(question);
        await db.SaveChangesAsync();
        
        return NoContent();
    }
}