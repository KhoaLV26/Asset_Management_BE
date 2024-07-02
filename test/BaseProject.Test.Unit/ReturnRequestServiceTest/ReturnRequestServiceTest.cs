using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.ReturnRequestServiceTest
{
    public class ReturnRequestServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAssignmentRepository> _mockAssignmentRepository;
        private readonly RequestReturnService _service;

        public ReturnRequestServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAssignmentRepository = new Mock<IAssignmentRepository>();
            _mockUnitOfWork.Setup(u => u.AssignmentRepository).Returns(_mockAssignmentRepository.Object);
            _service = new RequestReturnService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        
    }
}
