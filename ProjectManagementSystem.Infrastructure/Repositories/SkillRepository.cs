using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class SkillRepository(AppDbContext db, IMapper mapper) : ISkillRepository
{
    public async Task<IEnumerable<EmployeeSkillDto>> GetSkillsByEmployeeAsync(int employeeId)
    {
        var skills = await db.EmployeeSkills
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .ToListAsync();
        return mapper.Map<IEnumerable<EmployeeSkillDto>>(skills);
    }

    public async Task<EmployeeSkillDto> AddSkillAsync(int employeeId, AddSkillDto dto)
    {
        var skill = await db.Skills.FirstOrDefaultAsync(s => s.Name.ToLower() == dto.SkillName.ToLower());
        if (skill is null)
        {
            skill = new Skill
            {
                Name = dto.SkillName,
                Category = Enum.Parse<SkillCategory>(dto.Category, ignoreCase: true)
            };
            db.Skills.Add(skill);
            await db.SaveChangesAsync();
        }

        var already = await db.EmployeeSkills
            .AnyAsync(es => es.EmployeeId == employeeId && es.SkillId == skill.Id);
        if (already) throw new InvalidOperationException("Employee already has this skill.");

        var es = new EmployeeSkill
        {
            EmployeeId = employeeId,
            SkillId = skill.Id,
            ProficiencyLevel = Enum.Parse<ProficiencyLevel>(dto.ProficiencyLevel, ignoreCase: true)
        };
        db.EmployeeSkills.Add(es);
        await db.SaveChangesAsync();
        es.Skill = skill;
        return mapper.Map<EmployeeSkillDto>(es);
    }

    public async Task<EmployeeSkillDto> UpdateSkillAsync(int employeeId, int skillId, UpdateSkillDto dto)
    {
        var es = await db.EmployeeSkills
            .Include(e => e.Skill)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SkillId == skillId)
            ?? throw new KeyNotFoundException("Skill not found for this employee.");

        es.ProficiencyLevel = Enum.Parse<ProficiencyLevel>(dto.ProficiencyLevel, ignoreCase: true);
        await db.SaveChangesAsync();
        return mapper.Map<EmployeeSkillDto>(es);
    }

    public async Task RemoveSkillAsync(int employeeId, int skillId)
    {
        var es = await db.EmployeeSkills
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SkillId == skillId)
            ?? throw new KeyNotFoundException("Skill not found for this employee.");

        db.EmployeeSkills.Remove(es);
        await db.SaveChangesAsync();
    }
}
