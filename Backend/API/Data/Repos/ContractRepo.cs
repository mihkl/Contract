using API.FileManipulation;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repos;

public class ContractRepo(DataContext context) : IContractRepo
{
    private readonly DataContext _context = context;
    public async Task<Contract> Save(Contract contract, string? userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return null;
        }
        _context.Add(contract);
        user.Contracts.Add(contract);
        await SaveChangesAsync();
        return contract;
    }

    public async Task<List<Contract>> GetAll(string? userId)
    {
        var user = await _context.Users
            .Include(u => u.Contracts)
                .ThenInclude(c => c.Fields) 
            .Include(u => u.Contracts)
                .ThenInclude(c => c.SubmittedFields)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Contracts.ToList() ?? [];
    }

    public async Task<Contract?> GetById(uint id, string? userId)
    {
        var user = await _context.Users
            .Include(u => u.Contracts)
                .ThenInclude(c => c.Fields)
            .Include(u => u.Contracts)
                .ThenInclude(c => c.SubmittedFields)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Contracts.FirstOrDefault(c => c.Id == id);
    }

    public async Task<Contract?> GetById(uint id)
    {
        return await _context.Contracts
        .Include(c => c.Fields)
        .Include(c => c.SubmittedFields)
        .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task Delete(Contract contract)
    {
        _context.Remove(contract);
        await SaveChangesAsync();
    }

    public async Task ReplaceDynamicFields(List<ContractDynamicFieldReplacement> replacements, Contract contract, Template template)
    {
        contract.FileData = FileManipulator.ReplaceContractPlaceholders(replacements, template);
        UpdateDynamicFields(contract, replacements);

        await SaveChangesAsync();
    }

    private void UpdateDynamicFields(Contract contract, List<ContractDynamicFieldReplacement> replacements)
    {
        replacements.Where(r => contract.Fields.Any(f => f.Name == r.Name))
                            .ToList()
                            .ForEach(r => contract.Fields.First(f => f.Name == r.Name).Value = r.Value);

        _context.RemoveRange(contract.SubmittedFields);
        contract.SubmittedFields.Clear();
        contract.SubmittedFields.AddRange(replacements);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task Update(uint id, UpdateContract updateContract)
    {
        var contract = await GetById(id);
        var properties = typeof(UpdateContract).GetProperties();
        var contractProperties = typeof(Contract).GetProperties();

        foreach (var property in properties)
        {
            var dtoValue = property.GetValue(updateContract);
            if (dtoValue != null)
            {
                var entityProp = Array.Find(contractProperties, p => p.Name == property.Name);
                if (entityProp != null && entityProp.CanWrite)
                {
                    entityProp.SetValue(contract, dtoValue);
                }
            }
        }

        await SaveChangesAsync();
    }
}
