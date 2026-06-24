using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class SkillRepository(AppDbContext db, IMapper mapper) : ISkillRepository
{
    public async Task<IEnumerable<EmployeeSkillDto>> GetSkillsByEmployeeAsync(int employeeId)
    {
        var skills = await db.ResourceSkills
            .Include(rs => rs.Skill)
            .Where(rs => rs.ResourceId == employeeId)
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

        if (await db.ResourceSkills.AnyAsync(rs => rs.ResourceId == employeeId && rs.SkillId == skill.Id))
            throw new BusinessRuleException(ErrorMessages.EmployeeAlreadyHasSkill);

        var resourceSkill = mapper.Map<ResourceSkill>(dto);
        resourceSkill.ResourceId = employeeId;
        resourceSkill.SkillId = skill.Id;
        db.ResourceSkills.Add(resourceSkill);
        await db.SaveChangesAsync();
        resourceSkill.Skill = skill;
        return mapper.Map<EmployeeSkillDto>(resourceSkill);
    }

    public async Task<EmployeeSkillDto> UpdateSkillAsync(int employeeId, int skillId, UpdateSkillDto dto)
    {
        var resourceSkill = await db.ResourceSkills
            .Include(rs => rs.Skill)
            .FirstOrDefaultAsync(rs => rs.ResourceId == employeeId && rs.SkillId == skillId)
            ?? throw new NotFoundException(ErrorMessages.SkillNotFoundForEmployee());

        mapper.Map(dto, resourceSkill);
        await db.SaveChangesAsync();
        return mapper.Map<EmployeeSkillDto>(resourceSkill);
    }

    public async Task RemoveSkillAsync(int employeeId, int skillId)
    {
        var resourceSkill = await db.ResourceSkills
            .FirstOrDefaultAsync(rs => rs.ResourceId == employeeId && rs.SkillId == skillId)
            ?? throw new NotFoundException(ErrorMessages.SkillNotFoundForEmployee());

        db.ResourceSkills.Remove(resourceSkill);
        await db.SaveChangesAsync();
    }
}
