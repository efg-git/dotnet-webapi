using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.DTOs.Character;
using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _mapper = mapper;
        }
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceReponse = new ServiceResponse<List<GetCharacterDto>>();
            Character character = _mapper.Map<Character>(newCharacter);
            character.User = await _context.Users.FirstOrDefaultAsync(x => x.Id == GetUserId());
            _context.Characters.Add(character);
            await _context.SaveChangesAsync();
            serviceReponse.Data = await _context.Characters
                .Where(x => x.User.Id == GetUserId())
                .Select(x => _mapper.Map<GetCharacterDto>(x)).ToListAsync();

            return serviceReponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var serviceReponse = new ServiceResponse<List<GetCharacterDto>>();
            try
            {
                Character character = await _context.Characters.FirstOrDefaultAsync(x => x.Id == id && x.User.Id == GetUserId());
                if (character != null)
                {
                    _context.Characters.Remove(character);
                    await _context.SaveChangesAsync();
                    serviceReponse.Data = await _context.Characters
                        .Where(x => x.User.Id == GetUserId())
                        .Select(x => _mapper.Map<GetCharacterDto>(x)).ToListAsync();
                }
                else
                {
                    serviceReponse.Success = false;
                    serviceReponse.Message = "Character not found!";
                }
                
            }
            catch(Exception ex)
            {
                serviceReponse.Success = false;
                serviceReponse.Message = ex.Message;
            }

            return serviceReponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceReponse = new ServiceResponse<List<GetCharacterDto>>();
            var characters = await _context.Characters
                            .Include(x => x.Weapon)
                            .Include(x => x.Skills)
                            .Where(x => x.User.Id == GetUserId()).ToListAsync();
            serviceReponse.Data = characters.Select(x => _mapper.Map<GetCharacterDto>(x)).ToList();;

            return serviceReponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceReponse = new ServiceResponse<GetCharacterDto>();
            var characters = await _context.Characters
                            .Include(x => x.Weapon)
                            .Include(x => x.Skills)
                            .FirstOrDefaultAsync(x => x.Id == id && x.User.Id == GetUserId());
            serviceReponse.Data = _mapper.Map<GetCharacterDto>(characters);

            return serviceReponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var serviceReponse = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character character = await _context.Characters
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == updatedCharacter.Id);
                if (character.User.Id == GetUserId())
                {
                    character.Name = updatedCharacter.Name;
                    character.HitPoints = updatedCharacter.HitPoints;
                    character.Strength = updatedCharacter.Strength;
                    character.Defense = updatedCharacter.Defense;
                    character.Intelligence = updatedCharacter.Intelligence;
                    character.Class = updatedCharacter.Class;
                    
                    await _context.SaveChangesAsync();
                    serviceReponse.Data = _mapper.Map<GetCharacterDto>(character);
                }
                else
                {
                    serviceReponse.Success = false;
                    serviceReponse.Message = "Character not found!";
                }
                
            }
            catch(Exception ex)
            {
                serviceReponse.Success = false;
                serviceReponse.Message = ex.Message;
            }

            return serviceReponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
        {
            var response = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character character = await _context.Characters
                                        .Include(x => x.Weapon)
                                        .Include(x => x.Skills)
                                        .FirstOrDefaultAsync(x => x.Id == newCharacterSkill.CharacterId && x.User.Id == GetUserId());
                if (character == null)
                {
                    response.Success = false;
                    response.Message = "Character not found!";
                }

                var skill = await _context.Skills.FirstOrDefaultAsync(x => x.Id == newCharacterSkill.SkillId);
                if (skill == null)
                {
                    response.Success = false;
                    response.Message = "Skill not found!";
                }

                character.Skills.Add(skill);
                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}