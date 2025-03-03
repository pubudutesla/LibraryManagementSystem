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
            if (!Enum.TryParse<MembershipType>(registrationDto.MembershipType, true, out var membershipType))
            {
                throw new ArgumentException($"Invalid membership type '{registrationDto.MembershipType}'. Allowed values: {string.Join(", ", Enum.GetNames(typeof(MembershipType)))}");
            }

            string normalizedUsername = registrationDto.Username.ToLower();

            var existingMember = await _unitOfWork.Members.GetByUsernameAsync(normalizedUsername);

            if (existingMember != null)
            {
                throw new InvalidOperationException($"Username '{registrationDto.Username}' is already taken.");
            }

            var newMember = _mapper.Map<Member>(registrationDto);
            newMember.Username = normalizedUsername; // Normalize for case-insensitivity
            newMember.PasswordHash = HashPassword(registrationDto.Password); // Hash password manually

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
            if (existingMember == null) return false;

            existingMember.Name = updateDto.Name ?? existingMember.Name;
            existingMember.Email = updateDto.Email ?? existingMember.Email;

            if (!string.IsNullOrWhiteSpace(updateDto.MembershipType))
            {
                if (Enum.TryParse(updateDto.MembershipType, out MembershipType parsedType))
                {
                    existingMember.MembershipType = parsedType;
                }
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Password))
            {
                existingMember.PasswordHash = HashPassword(updateDto.Password);
            }

            _unitOfWork.Members.UpdateAsync(existingMember);
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

        // ✅ Manual SHA-256 password hashing (No external libraries)
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}