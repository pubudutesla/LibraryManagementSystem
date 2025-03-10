﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Api.Controllers;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Services;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Tests.Controllers
{
    public class MemberControllerTests
    {
        private readonly Mock<IMemberService> _mockMemberService;
        private readonly Mock<ILogger<MemberController>> _mockLogger;
        private readonly MemberController _controller;

        public MemberControllerTests()
        {
            _mockMemberService = new Mock<IMemberService>();
            // Initialize the logger mock!
            _mockLogger = new Mock<ILogger<MemberController>>();

            // Pass both mocks to the controller's constructor
            _controller = new MemberController(_mockMemberService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetMembers_ShouldReturnOk_WithMembers()
        {
            // Arrange
            var members = new List<MemberResponseDto>
            {
                new MemberResponseDto { Id = 1, Username = "admin", Name = "Admin User" },
                new MemberResponseDto { Id = 2, Username = "librarian", Name = "Librarian User" }
            };

            _mockMemberService
                .Setup(s => s.GetAllMembersAsync())
                .ReturnsAsync(members);

            // Act
            var result = await _controller.GetMembers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedMembers = Assert.IsAssignableFrom<IEnumerable<MemberResponseDto>>(okResult.Value);
            Assert.Equal(2, returnedMembers.Count());
        }

        [Fact]
        public async Task GetMemberById_ShouldReturnNotFound_WhenMemberDoesNotExist()
        {
            // Arrange
            _mockMemberService
                .Setup(s => s.GetMemberByIdAsync(999))
                .ReturnsAsync((MemberResponseDto?)null);

            // Act
            var result = await _controller.GetMemberById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddMember_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var registrationDto = new MemberRegistrationDto
            {
                Username = "testuser",
                Name = "Test User",
                Password = "password",
                MembershipType = "Member"
            };

            var memberResponseDto = new MemberResponseDto
            {
                Id = 1,
                Username = "testuser",
                Name = "Test User"
            };

            _mockMemberService
                .Setup(s => s.AddMemberAsync(registrationDto))
                .ReturnsAsync(memberResponseDto);

            // Act
            var result = await _controller.AddMember(registrationDto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetMemberById", actionResult.ActionName);
        }

        [Fact]
        public async Task DeleteMember_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            _mockMemberService
                .Setup(s => s.DeleteMemberAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteMember(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}