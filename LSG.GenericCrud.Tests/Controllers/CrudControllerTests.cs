﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using LSG.GenericCrud.Controllers;
using LSG.GenericCrud.Exceptions;
using LSG.GenericCrud.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LSG.GenericCrud.Tests.Controllers
{
    
    public class CrudControllerTests
    {
        private IList<TestEntity> _entities;
        private Faker<TestEntity> _entityFaker;

        public CrudControllerTests()
        {
            Randomizer.Seed = new Random(1234567);
            _entityFaker = new Faker<TestEntity>().
                    RuleFor(_ => _.Id, Guid.NewGuid()).
                    RuleFor(_ => _.Value, _ => _.Lorem.Word());
            _entities = _entityFaker.
                Generate(5);
        }

        [Fact]
        public void GetAll_ReturnsOk()
        {
            var dalMock = new Mock<Crud<TestEntity>>();
            dalMock.Setup(_ => _.GetAll()).Returns(_entities);
            var controller = new CrudController<TestEntity>(dalMock.Object);

            var actionResult = controller.GetAll();
            var okResult = actionResult as OkObjectResult;
            var model = okResult.Value as IEnumerable<TestEntity>;

            Assert.Equal(model.Count(), _entities.Count);
            dalMock.Verify(_ => _.GetAll(), Times.Once);
        }

        [Fact]
        public void GetById_ReturnsOk()
        {
            var id = _entities[0].Id;
            var dalMock = new Mock<Crud<TestEntity>>();
            dalMock.Setup(_ => _.GetById(id)).Returns(_entities[0]);
            var controller = new CrudController<TestEntity>(dalMock.Object);
            
            var actionResult = controller.GetById(id);
            var okResult = actionResult as OkObjectResult;
            var model = okResult.Value as TestEntity;

            Assert.Equal(model.Id, id);
            dalMock.Verify(_ => _.GetById(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void GetById_ReturnsNotFound()
        {
            var dalMock = new Mock<Crud<TestEntity>>();
            dalMock.Setup(_ => _.GetById(It.IsAny<Guid>())).Throws(new EntityNotFoundException());
            var controller = new CrudController<TestEntity>(dalMock.Object);
            
            var actionResult = controller.GetById(Guid.NewGuid());
            
            Assert.IsType(typeof(NotFoundResult), actionResult);
            dalMock.Verify(_ => _.GetById(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void Create_ReturnsCreatedEntity()
        {
            var entity = _entityFaker.Generate();
            var dalMock = new Mock<Crud<TestEntity>>();
            var controller = new CrudController<TestEntity>(dalMock.Object);

            var actionResult = controller.Create(entity);

            Assert.IsType<OkObjectResult>(actionResult);
            dalMock.Verify(_ => _.Create(It.IsAny<TestEntity>()), Times.Once);
        }

        [Fact]
        public void Update_ReturnsModifiedEntity()
        {
            var entity = _entityFaker.Generate();
            var dalMock = new Mock<Crud<TestEntity>>();
            var controller = new CrudController<TestEntity>(dalMock.Object);

            var actionResult = controller.Update(entity.Id, entity);

            Assert.IsType<OkResult>(actionResult);
            dalMock.Verify(_ => _.Update(It.IsAny<Guid>(), It.IsAny<TestEntity>()), Times.Once);
        }

        [Fact]
        public void Update_ReturnsNotFound()
        {
            var entity = _entityFaker.Generate();
            var dalMock = new Mock<Crud<TestEntity>>();
            dalMock.Setup(_ => _.Update(It.IsAny<Guid>(), It.IsAny<TestEntity>())).Throws<EntityNotFoundException>();
            var controller = new CrudController<TestEntity>(dalMock.Object);

            var actionResult = controller.Update(entity.Id, entity);

            Assert.IsType(typeof(NotFoundResult), actionResult);
            dalMock.Verify(_ => _.Update(It.IsAny<Guid>(), It.IsAny<TestEntity>()), Times.Once);
        }

        [Fact]
        public void Delete_ReturnsOk()
        {
            var entity = _entityFaker.Generate();
            var dalMock = new Mock<Crud<TestEntity>>();
            var controller = new CrudController<TestEntity>(dalMock.Object);

            var actionResult = controller.Delete(entity.Id);

            Assert.IsType(typeof(OkResult), actionResult);
            dalMock.Verify(_ => _.Delete(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void Delete_ReturnsNotFound()
        {
            var entity = _entityFaker.Generate();
            var dalMock = new Mock<Crud<TestEntity>>();
            dalMock.Setup(_ => _.Delete(It.IsAny<Guid>())).Throws<EntityNotFoundException>();
            var controller = new CrudController<TestEntity>(dalMock.Object);

            var actionResult = controller.Delete(entity.Id);

            Assert.IsType(typeof(NotFoundResult), actionResult);
            dalMock.Verify(_ => _.Delete(It.IsAny<Guid>()), Times.Once);
        }
    }
}
