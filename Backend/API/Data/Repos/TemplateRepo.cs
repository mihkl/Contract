using API.FileManipulation;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repos;

public class TemplateRepo(DataContext context) : ITemplateRepo
{
    private readonly DataContext _context = context;
    public async Task<Template> Save(Template template)
    {
        template.CreationTime = DateTime.UtcNow;
        
        _context.Add(template);
        await SaveChangesAsync();
        return template;
    }

    public async Task<List<Template>> GetAll(string? userId)
    {
        var templates = _context.Templates.AsQueryable();
        if (!string.IsNullOrEmpty(userId))
        {
            templates = templates.Where(c => c.UserId == userId);
        }
        return await templates
        .Include(t => t.Fields)
        .ToListAsync();
    }

    public async Task<Template?> GetById(uint id, string? userId)
    {
        var templates = _context.Templates.AsQueryable();
        if (!string.IsNullOrEmpty(userId))
        {
            templates = templates.Where(c => c.UserId == userId);
        }
        return await templates
        .Include(t => t.Fields)
        .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Template?> GetById(uint id)
    {
        return await _context.Templates
        .Include(t => t.Fields)
        .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task Delete(Template template)
    {
        _context.Remove(template);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
