using Xunit;
using Moq;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;
using LibraryManagementSystem.Application.Mapping;

namespace LibraryManagementSystem.Tests.Services
{
    public class MemberServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMemberRepository> _mockMemberRepository;
        private readonly IMapper _mapper;
        private readonly MemberService _memberService;

        public MemberServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMemberRepository = new Mock<IMemberRepository>();

            _mockUnitOfWork.Setup(u => u.Members).Returns(_mockMemberRepository.Object);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();

            _memberService = new MemberService(_mockUnitOfWork.Object, _mapper);
        }

        [Fact]
        public async Task GetAllMembersAsync_ShouldReturnListOfMembers()
        {
            // Arrange
            var members = new List<Member>
            {
                new Member("admin", "Admin User", "admin@example.com", "hashedpassword", MembershipType.Admin),
                new Member("librarian", "Librarian User", "librarian@example.com", "hashedpassword", MembershipType.Librarian)
            };
            _mockMemberRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(members);

            // Act
            var result = await _memberService.GetAllMembersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetMemberByIdAsync_ShouldReturnMember_WhenExists()
        {
            // Arrange
            var member = new Member("admin", "Admin User", "admin@example.com", "passhash", MembershipType.Admin);
            // Suppose EF set an Id if needed:
            var memberType = typeof(Member);
            memberType.GetProperty(nameof(Member.Id))!.SetValue(member, 1);

            _mockMemberRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

            // Act
            var result = await _memberService.GetMemberByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("admin", result.Username);
        }

        [Fact]
        public async Task AddMemberAsync_ShouldThrowException_WhenUsernameExists()
        {
            // Arrange
            var registrationDto = new MemberRegistrationDto
            {
                Username = "admin",
                Name = "Admin User",
                MembershipType = "Admin",
                Password = "pass123",
                Email = "admin@example.com"
            };

            _mockMemberRepository.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(
                new Member("admin", "Admin User", "admin@example.com", "hash", MembershipType.Admin)
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _memberService.AddMemberAsync(registrationDto));
        }

        [Fact]
        public async Task AddMemberAsync_ShouldCreateMember_WhenValid()
        {
            // Arrange
            var registrationDto = new MemberRegistrationDto
            {
                Username = "newuser",
                Name = "New User",
                MembershipType = "Member",
                Password = "securepassword",
                Email = "newuser@example.com"
            };

            _mockMemberRepository.Setup(r => r.GetByUsernameAsync("newuser")).ReturnsAsync((Member?)null);

            // Act
            var result = await _memberService.AddMemberAsync(registrationDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newuser", result.Username);
        }

        [Fact]
        public async Task UpdateMemberAsync_ShouldUpdateExistingMember()
        {
            // Arrange
            var updateDto = new MemberUpdateDto { Name = "Updated Name" };
            var existingMember = new Member("admin", "Old Name", "admin@example.com", "passhash", MembershipType.Admin);
            var memberType = typeof(Member);
            memberType.GetProperty(nameof(Member.Id))!.SetValue(existingMember, 1);

            _mockMemberRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingMember);
            _mockMemberRepository.Setup(r => r.UpdateAsync(existingMember));

            // Act
            var result = await _memberService.UpdateMemberAsync(1, updateDto);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated Name", existingMember.Name);
        }

        [Fact]
        public async Task DeleteMemberAsync_ShouldReturnFalse_WhenMemberDoesNotExist()
        {
            // Arrange
            _mockMemberRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Member?)null);

            // Act
            var result = await _memberService.DeleteMemberAsync(1);

            // Assert
            Assert.False(result);
        }
    }
}