using LibraryManagementSystem.Application.DTOs;

namespace LibraryManagementSystem.Application.Services
{
    public interface IMemberService
    {
        Task<IEnumerable<MemberResponseDto>> GetAllMembersAsync();
        Task<MemberResponseDto?> GetMemberByIdAsync(int id);
        Task<MemberResponseDto?> GetMemberByUsernameAsync(string username);
        Task<MemberResponseDto> AddMemberAsync(MemberRegistrationDto registrationDto);
        Task<bool> UpdateMemberAsync(int id, MemberUpdateDto updateDto);
        Task<bool> DeleteMemberAsync(int id);
    }
}