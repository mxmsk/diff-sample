using System;
using System.Linq;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Controllers;
using DiffSample.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace DiffSample.Test.Controllers
{
    [TestClass]
    public class DiffControllerTest
    {
        [TestMethod]
        public async Task GetReturnsNotFoundResultWhenReadinessNotFound()
        {
            var service = Substitute.For<IDifferenceService>();
            service.FindDiffAsync(10).Returns(
                Task.FromResult(((DifferenceContent)null, DifferenceReadiness.NotFound)));

            var controller = new DiffController(service);
            var result = await controller.Get(10);

            result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task GetReturnsNoContentResultWhenReadinessNotReady()
        {
            var service = Substitute.For<IDifferenceService>();
            service.FindDiffAsync(10).Returns(
                Task.FromResult(((DifferenceContent)null, DifferenceReadiness.NotReady)));

            var controller = new DiffController(service);
            var result = await controller.Get(10);

            result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task GetReturnsOkObjectResultWithDifferenceContentWhenReadinessReady()
        {
            var content = new DifferenceContent
            {
                Type = DifferenceType.SizeDiffers
            };

            var service = Substitute.For<IDifferenceService>();
            service.FindDiffAsync(10).Returns(
                Task.FromResult((content, DifferenceReadiness.Ready)));

            var controller = new DiffController(service);
            var result = await controller.Get(10);

            result.Should()
                  .BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(content);
        }

        [TestMethod]
        public async Task PostLeftReturnsBadRequestResultWhenModelIsInvalid()
        {
            var service = Substitute.For<IDifferenceService>();

            var controller = new DiffController(service);
            controller.ModelState.AddModelError("error", "some error");

            var result = await controller.PostLeft(10, new SourceContentRequest());

            result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task PostLeftReturnsBadRequestResultWhenModelDataIsNotBase64()
        {
            var service = Substitute.For<IDifferenceService>();
            var request = new SourceContentRequest
            {
                Data = "__."
            };

            var controller = new DiffController(service);
            var result = await controller.PostLeft(10, request);

            result.Should().BeOfType<BadRequestResult>();
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(20)]
        public async Task PostLeftAddsSourceForGivenDiff(int diffId)
        {
            var data = new byte[] { 1, 2, 3 };
            var service = Substitute.For<IDifferenceService>();
            var request = new SourceContentRequest
            {
                Data = Convert.ToBase64String(data)
            };

            var controller = new DiffController(service);
            var result = await controller.PostLeft(diffId, request);

            result.Should().BeOfType<OkResult>();
            await service.Received().AddSourceAsync(diffId, Arg.Is<SourceContent>(
                c => c.SourceSide == SourceSide.Left && c.Data.SequenceEqual(data)));
        }

        [TestMethod]
        public async Task PostRightReturnsBadRequestResultWhenModelIsInvalid()
        {
            var service = Substitute.For<IDifferenceService>();

            var controller = new DiffController(service);
            controller.ModelState.AddModelError("error", "some error");

            var result = await controller.PostRight(10, new SourceContentRequest());

            result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task PostRightReturnsBadRequestResultWhenModelDataIsNotBase64()
        {
            var service = Substitute.For<IDifferenceService>();
            var request = new SourceContentRequest
            {
                Data = "__."
            };

            var controller = new DiffController(service);
            var result = await controller.PostRight(10, request);

            result.Should().BeOfType<BadRequestResult>();
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(20)]
        public async Task PostRightAddsSourceForGivenDiff(int diffId)
        {
            var data = new byte[] { 1, 2, 3 };
            var service = Substitute.For<IDifferenceService>();
            var request = new SourceContentRequest
            {
                Data = Convert.ToBase64String(data)
            };

            var controller = new DiffController(service);
            var result = await controller.PostRight(diffId, request);

            result.Should().BeOfType<OkResult>();
            await service.Received().AddSourceAsync(diffId, Arg.Is<SourceContent>(
                c => c.SourceSide == SourceSide.Right && c.Data.SequenceEqual(data)));
        }
    }
}
