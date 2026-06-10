using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
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
            skill = mapper.Map<Skill>(dto);
            db.Skills.Add(skill);
            await db.SaveChangesAsync();
        }

        var already = await db.EmployeeSkills
            .AnyAsync(es => es.EmployeeId == employeeId && es.SkillId == skill.Id);
        if (already) throw new InvalidOperationException(ErrorMessages.EmployeeAlreadyHasSkill);

        var es = mapper.Map<EmployeeSkill>(dto);
        es.EmployeeId = employeeId;
        es.SkillId = skill.Id;
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
            ?? throw new KeyNotFoundException(ErrorMessages.SkillNotFoundForEmployee());

        mapper.Map(dto, es);
        await db.SaveChangesAsync();
        return mapper.Map<EmployeeSkillDto>(es);
    }

    public async Task RemoveSkillAsync(int employeeId, int skillId)
    {
        var es = await db.EmployeeSkills
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SkillId == skillId)
            ?? throw new KeyNotFoundException(ErrorMessages.SkillNotFoundForEmployee());

        db.EmployeeSkills.Remove(es);
        await db.SaveChangesAsync();
    }
}
