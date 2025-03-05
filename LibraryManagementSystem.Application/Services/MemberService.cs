using AutoMapper;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Application.Services
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MemberService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MemberResponseDto>> GetAllMembersAsync()
        {
            var members = await _unitOfWork.Members.GetAllAsync();
            return _mapper.Map<IEnumerable<MemberResponseDto>>(members);
        }

        public async Task<MemberResponseDto?> GetMemberByIdAsync(int id)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(id);
            return member == null ? null : _mapper.Map<MemberResponseDto>(member);
        }

        public async Task<MemberResponseDto> AddMemberAsync(MemberRegistrationDto registrationDto)
        {
            if (!Enum.TryParse(registrationDto.MembershipType, true, out MembershipType membershipType))
            {
                throw new ArgumentException(
                    $"Invalid membership type '{registrationDto.MembershipType}'. " +
                    $"Allowed values: {string.Join(", ", Enum.GetNames(typeof(MembershipType)))}");
            }

            var normalizedUsername = registrationDto.Username.ToLower();
            var existingMember = await _unitOfWork.Members.GetByUsernameAsync(normalizedUsername);
            if (existingMember != null)
            {
                throw new InvalidOperationException(
                    $"Username '{registrationDto.Username}' is already taken.");
            }

            // Create a domain Member object
            var newMember = new Member(
                username: normalizedUsername,
                name: registrationDto.Name,
                email: registrationDto.Email,
                passwordHash: HashPassword(registrationDto.Password),
                membershipType: membershipType
            );

            await _unitOfWork.Members.AddAsync(newMember);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<MemberResponseDto>(newMember);
        }

        public async Task<MemberResponseDto?> GetMemberByUsernameAsync(string username)
        {
            var member = await _unitOfWork.Members.GetByUsernameAsync(username);
            return member == null ? null : _mapper.Map<MemberResponseDto>(member);
        }

        public async Task<bool> UpdateMemberAsync(int id, MemberUpdateDto updateDto)
        {
            var existingMember = await _unitOfWork.Members.GetByIdAsync(id);
            if (existingMember == null) return false; // let controller 404

            // If you have the domain method `Update(...)`:
            MembershipType? membershipTypeParsed = null;
            if (!string.IsNullOrWhiteSpace(updateDto.MembershipType) &&
                Enum.TryParse(updateDto.MembershipType, out MembershipType parsedType))
            {
                membershipTypeParsed = parsedType;
            }

            // Domain approach: calls entity method
            existingMember.Update(
                newName: updateDto.Name,
                newEmail: updateDto.Email,
                newPasswordHash: !string.IsNullOrWhiteSpace(updateDto.Password)
                    ? HashPassword(updateDto.Password)
                    : null,
                newMembershipType: membershipTypeParsed
            );

            _unitOfWork.Members.UpdateAsync(existingMember); // or just Update(existingMember)
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMemberAsync(int id)
        {
            var existingMember = await _unitOfWork.Members.GetByIdAsync(id);
            if (existingMember == null) return false;

            await _unitOfWork.Members.DeleteAsync(existingMember);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}