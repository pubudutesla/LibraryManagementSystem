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
        private readonly IMapper _mapper;
        private readonly MemberService _memberService;

        public MemberServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();

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
                new Member { Id = 1, Username = "admin", Name = "Admin User", MembershipType = MembershipType.Admin },
                new Member { Id = 2, Username = "librarian", Name = "Librarian User", MembershipType = MembershipType.Librarian }
            };
            _mockUnitOfWork.Setup(u => u.Members.GetAllAsync()).ReturnsAsync(members);

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
            var member = new Member { Id = 1, Username = "admin", Name = "Admin User", MembershipType = MembershipType.Admin };
            _mockUnitOfWork.Setup(u => u.Members.GetByIdAsync(1)).ReturnsAsync(member);

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
                Password = "pass123"
            };

            _mockUnitOfWork.Setup(u => u.Members.GetByUsernameAsync("admin")).ReturnsAsync(new Member());

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
                Password = "securepassword"
            };

            _mockUnitOfWork.Setup(u => u.Members.GetByUsernameAsync("newuser")).ReturnsAsync((Member)null);
            _mockUnitOfWork.Setup(u => u.Members.AddAsync(It.IsAny<Member>()));

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
            var existingMember = new Member { Id = 1, Username = "admin", Name = "Old Name", MembershipType = MembershipType.Admin };

            _mockUnitOfWork.Setup(u => u.Members.GetByIdAsync(1)).ReturnsAsync(existingMember);
            _mockUnitOfWork.Setup(u => u.Members.UpdateAsync(existingMember));

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
            _mockUnitOfWork.Setup(u => u.Members.GetByIdAsync(1)).ReturnsAsync((Member)null);

            // Act
            var result = await _memberService.DeleteMemberAsync(1);

            // Assert
            Assert.False(result);
        }
    }
}