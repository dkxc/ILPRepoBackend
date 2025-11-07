using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Phases;
using IlpRepoBackend.Application.Query.Phases;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Phases
{
    public class GetPhasesByBatchIdHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPhasesByBatchIdHandler _handler;

        public GetPhasesByBatchIdHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetPhasesByBatchIdHandler(_batchRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchWithPhases_ReturnsPhases()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch 2024",
                Phases = new List<Phase>
                {
                    new Phase 
                    { 
                        Id = 1, 
                        PhaseType = "Foundation Phase", 
                        PhaseTypeId = 1,
                        StartDate = DateTime.UtcNow.AddDays(-30),
                        EndDate = DateTime.UtcNow.AddDays(-1),
                        BatchId = batchId
                    },
                    new Phase 
                    { 
                        Id = 2, 
                        PhaseType = "Advanced Phase", 
                        PhaseTypeId = 2,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(30),
                        BatchId = batchId
                    }
                }
            };

            var phaseDtos = new List<PhaseDto>
            {
                new PhaseDto 
                { 
                    Id = 1, 
                    PhaseType = "Foundation Phase",
                    PhaseTypeId = 1,
                    StartDate = batch.Phases.ElementAt(0).StartDate,
                    EndDate = batch.Phases.ElementAt(0).EndDate
                },
                new PhaseDto 
                { 
                    Id = 2, 
                    PhaseType = "Advanced Phase",
                    PhaseTypeId = 2,
                    StartDate = batch.Phases.ElementAt(1).StartDate,
                    EndDate = batch.Phases.ElementAt(1).EndDate
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(2);
            result.Data[0].PhaseType.ShouldBe("Foundation Phase");
            result.Data[1].PhaseType.ShouldBe("Advanced Phase");
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsFailure()
        {
            // Arrange
            var batchId = 999;
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync((Batch)null);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe($"Batch with ID {batchId} not found");
            result.Data.ShouldBeNull();

            _mapperMock.Verify(x => x.Map<List<PhaseDto>>(It.IsAny<ICollection<Phase>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BatchWithNoPhases_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Empty Batch",
                Phases = new List<Phase>()
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("No phases found for the specified batch");
            result.Data.ShouldBeNull();

            _mapperMock.Verify(x => x.Map<List<PhaseDto>>(It.IsAny<ICollection<Phase>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BatchWithNullPhases_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Batch Without Phases",
                Phases = null
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("No phases found for the specified batch");
        }

        [Fact]
        public async Task Handle_SinglePhase_ReturnsSinglePhase()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Single Phase Batch",
                Phases = new List<Phase>
                {
                    new Phase 
                    { 
                        Id = 1, 
                        PhaseType = "Training Phase",
                        PhaseTypeId = 1,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(3),
                        BatchId = batchId
                    }
                }
            };

            var phaseDtos = new List<PhaseDto>
            {
                new PhaseDto 
                { 
                    Id = 1, 
                    PhaseType = "Training Phase",
                    PhaseTypeId = 1,
                    StartDate = batch.Phases.First().StartDate,
                    EndDate = batch.Phases.First().EndDate
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].PhaseType.ShouldBe("Training Phase");
        }

        [Fact]
        public async Task Handle_MultiplePhases_ReturnsAllPhases()
        {
            // Arrange
            var batchId = 1;
            var phases = Enumerable.Range(1, 5)
                .Select(i => new Phase
                {
                    Id = i,
                    PhaseType = $"Phase {i}",
                    PhaseTypeId = i,
                    StartDate = DateTime.UtcNow.AddMonths(i - 1),
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    BatchId = batchId
                })
                .ToList();

            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Multi-Phase Batch",
                Phases = phases
            };

            var phaseDtos = phases.Select(p => new PhaseDto
            {
                Id = p.Id,
                PhaseType = p.PhaseType,
                PhaseTypeId = p.PhaseTypeId,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).ToList();

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(5);
            result.Data.ShouldAllBe(dto => !string.IsNullOrEmpty(dto.PhaseType));
        }

        [Fact]
        public async Task Handle_PhasesWithDifferentDateRanges_ReturnsCorrectDates()
        {
            // Arrange
            var batchId = 1;
            var now = DateTime.UtcNow;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Date Range Test Batch",
                Phases = new List<Phase>
                {
                    new Phase 
                    { 
                        Id = 1, 
                        PhaseType = "Past Phase",
                        StartDate = now.AddMonths(-2),
                        EndDate = now.AddMonths(-1),
                        BatchId = batchId
                    },
                    new Phase 
                    { 
                        Id = 2, 
                        PhaseType = "Current Phase",
                        StartDate = now.AddDays(-15),
                        EndDate = now.AddDays(15),
                        BatchId = batchId
                    },
                    new Phase 
                    { 
                        Id = 3, 
                        PhaseType = "Future Phase",
                        StartDate = now.AddMonths(1),
                        EndDate = now.AddMonths(2),
                        BatchId = batchId
                    }
                }
            };

            var phaseDtos = batch.Phases.Select(p => new PhaseDto
            {
                Id = p.Id,
                PhaseType = p.PhaseType,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).ToList();

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
            
            var pastPhase = result.Data.FirstOrDefault(p => p.PhaseType == "Past Phase");
            pastPhase.ShouldNotBeNull();
            pastPhase.EndDate.ShouldBeLessThan(now);

            var futurePhase = result.Data.FirstOrDefault(p => p.PhaseType == "Future Phase");
            futurePhase.ShouldNotBeNull();
            futurePhase.StartDate.ShouldBeGreaterThan(now);
        }

        [Fact]
        public async Task Handle_PhasesWithPhaseTypeId_MapsCorrectly()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase>
                {
                    new Phase 
                    { 
                        Id = 1, 
                        PhaseType = "Phase A", 
                        PhaseTypeId = 10,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(1),
                        BatchId = batchId
                    }
                }
            };

            var phaseDtos = new List<PhaseDto>
            {
                new PhaseDto 
                { 
                    Id = 1, 
                    PhaseType = "Phase A",
                    PhaseTypeId = 10,
                    StartDate = batch.Phases.First().StartDate,
                    EndDate = batch.Phases.First().EndDate
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].PhaseTypeId.ShouldBe(10);
        }

        [Fact]
        public async Task Handle_PhasesWithNullPhaseTypeId_HandlesCorrectly()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase>
                {
                    new Phase 
                    { 
                        Id = 1, 
                        PhaseType = "Custom Phase", 
                        PhaseTypeId = null,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(1),
                        BatchId = batchId
                    }
                }
            };

            var phaseDtos = new List<PhaseDto>
            {
                new PhaseDto 
                { 
                    Id = 1, 
                    PhaseType = "Custom Phase",
                    PhaseTypeId = null,
                    StartDate = batch.Phases.First().StartDate,
                    EndDate = batch.Phases.First().EndDate
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].PhaseTypeId.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase>
                {
                    new Phase { Id = 1, PhaseType = "Phase 1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(It.IsAny<ICollection<Phase>>()))
                .Returns(new List<PhaseDto> { new PhaseDto() });

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(batchId), Times.Once);
        }

        [Fact]
        public async Task Handle_CallsMapperOnce_WhenPhasesExist()
        {
            // Arrange
            var batchId = 1;
            var phases = new List<Phase>
            {
                new Phase { Id = 1, PhaseType = "Phase 1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) }
            };

            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = phases
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(phases))
                .Returns(new List<PhaseDto> { new PhaseDto() });

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mapperMock.Verify(x => x.Map<List<PhaseDto>>(phases), Times.Once);
        }

        [Fact]
        public async Task Handle_PhasesWithLongNames_HandlesCorrectly()
        {
            // Arrange
            var batchId = 1;
            var longPhaseName = "Very Long Phase Name For Testing Maximum Length Handling In The System";
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase>
                {
                    new Phase 
                    { 
                        Id = 1, 
                        PhaseType = longPhaseName,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(1),
                        BatchId = batchId
                    }
                }
            };

            var phaseDtos = new List<PhaseDto>
            {
                new PhaseDto { Id = 1, PhaseType = longPhaseName }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].PhaseType.ShouldBe(longPhaseName);
            result.Data[0].PhaseType.Length.ShouldBeGreaterThan(50);
        }

        [Fact]
        public async Task Handle_SequentialPhases_ReturnsInCorrectOrder()
        {
            // Arrange
            var batchId = 1;
            var now = DateTime.UtcNow;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Sequential Batch",
                Phases = new List<Phase>
                {
                    new Phase 
                    { 
                        Id = 1, 
                        PhaseType = "Phase 1",
                        StartDate = now,
                        EndDate = now.AddMonths(1),
                        BatchId = batchId
                    },
                    new Phase 
                    { 
                        Id = 2, 
                        PhaseType = "Phase 2",
                        StartDate = now.AddMonths(1),
                        EndDate = now.AddMonths(2),
                        BatchId = batchId
                    },
                    new Phase 
                    { 
                        Id = 3, 
                        PhaseType = "Phase 3",
                        StartDate = now.AddMonths(2),
                        EndDate = now.AddMonths(3),
                        BatchId = batchId
                    }
                }
            };

            var phaseDtos = batch.Phases.Select(p => new PhaseDto
            {
                Id = p.Id,
                PhaseType = p.PhaseType,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).ToList();

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
            
            // Verify phases are sequential (next phase starts when previous ends)
            for (int i = 0; i < result.Data.Count - 1; i++)
            {
                result.Data[i].EndDate.ShouldBeLessThanOrEqualTo(result.Data[i + 1].StartDate);
            }
        }

        [Fact]
        public async Task Handle_OverlappingPhases_ReturnsAllPhases()
        {
            // Arrange
            var batchId = 1;
            var now = DateTime.UtcNow;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Overlapping Batch",
                Phases = new List<Phase>
                {
                    new Phase 
                    { 
                        Id = 1, 
                        PhaseType = "Main Phase",
                        StartDate = now,
                        EndDate = now.AddMonths(3),
                        BatchId = batchId
                    },
                    new Phase 
                    { 
                        Id = 2, 
                        PhaseType = "Parallel Phase",
                        StartDate = now.AddMonths(1),
                        EndDate = now.AddMonths(2),
                        BatchId = batchId
                    }
                }
            };

            var phaseDtos = batch.Phases.Select(p => new PhaseDto
            {
                Id = p.Id,
                PhaseType = p.PhaseType,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).ToList();

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(batch.Phases)).Returns(phaseDtos);

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            
            // Verify overlapping dates
            var mainPhase = result.Data.First(p => p.PhaseType == "Main Phase");
            var parallelPhase = result.Data.First(p => p.PhaseType == "Parallel Phase");
            
            parallelPhase.StartDate.ShouldBeGreaterThanOrEqualTo(mainPhase.StartDate);
            parallelPhase.EndDate.ShouldBeLessThanOrEqualTo(mainPhase.EndDate);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task Handle_DifferentBatchIds_CallsCorrectBatch(int batchId)
        {
            // Arrange
            var batch = new Batch
            {
                Id = batchId,
                BatchName = $"Batch {batchId}",
                Phases = new List<Phase>
                {
                    new Phase { Id = 1, PhaseType = "Test Phase", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<List<PhaseDto>>(It.IsAny<ICollection<Phase>>()))
                .Returns(new List<PhaseDto> { new PhaseDto() });

            var query = new GetPhasesByBatchIdQuery(batchId);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(batchId), Times.Once);
        }
    }
}
