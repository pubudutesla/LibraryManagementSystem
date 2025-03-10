﻿using LibraryManagementSystem.Application.DTOs;

namespace LibraryManagementSystem.Application.Services
{
    public interface ILoanService
    {
        Task<IEnumerable<LoanResponseDto>> GetAllLoansAsync();
        Task<LoanResponseDto?> GetLoanByIdAsync(int id);
        Task<LoanResponseDto> CreateLoanAsync(LoanRequestDto loanDto);
        Task<IEnumerable<LoanResponseDto>> GetLoansByMemberIdAsync(int memberId);
        Task<bool> ReturnBookAsync(int id);
    }
}