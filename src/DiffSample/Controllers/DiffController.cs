using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using DiffSample.Model;
using System;

namespace DiffSample.Controllers
{
    /// <summary>
    /// Provides actions to manipulate diffs.
    /// </summary>
    [Route("v1/[controller]")]
    public sealed class DiffController : Controller
    {
        private readonly IDifferenceService _differenceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffController"/> class.
        /// </summary>
        /// <param name="differenceService">The service that implements difference.</param>
        public DiffController(IDifferenceService differenceService)
        {
            _differenceService = differenceService;
        }

        /// <summary>
        /// Get the diff with the specified id.
        /// </summary>
        /// <param name="id">Identifier of requested diff.</param>
        /// <returns>The task that handles the request.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var (diff, readiness) = await _differenceService.FindDiffAsync(id);

            switch (readiness)
            {
                case DifferenceReadiness.NotFound:
                    return NotFound();
                case DifferenceReadiness.NotReady:
                    return NoContent();
                default:
                    return Ok(diff);
            }
        }

        /// <summary>
        /// Receives source from the left side for difference.
        /// </summary>
        /// <param name="id">Identifier of diff which source belongs to.</param>
        /// <param name="model">The object containing request data.</param>
        /// <returns>The task that handles the request.</returns>
        [HttpPost("{id}/left")]
        public Task<IActionResult> PostLeft(int id, [FromBody]SourceContentRequest model)
        {
            return ReceiveSourceContent(id, SourceSide.Left, model);
        }

        /// <summary>
        /// Receives source from the right side for difference.
        /// </summary>
        /// <param name="id">Identifier of diff which source belongs to.</param>
        /// <param name="model">The object containing request data.</param>
        /// <returns>The task that handles the request.</returns>
        [HttpPost("{id}/right")]
        public Task<IActionResult> PostRight(int id, [FromBody]SourceContentRequest model)
        {
            return ReceiveSourceContent(id, SourceSide.Right, model);
        }

        private async Task<IActionResult> ReceiveSourceContent(
            int id, SourceSide side, SourceContentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var content = new SourceContent { SourceSide = side };
            try
            {
                content.Data = Convert.FromBase64String(model.Data);
            }
            catch (FormatException)
            {
                return BadRequest();
            }

            await _differenceService.AddSourceAsync(id, content);
            return Ok();
        }
    }
}
