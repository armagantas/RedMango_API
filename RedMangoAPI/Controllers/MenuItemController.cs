using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using System.Net;

namespace RedMangoAPI.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private ApiResponse _response;

        public MenuItemController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _response = new ApiResponse();
        }


        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            _response.Result = _dbContext.MenuItems;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMenuItemById(int id)
        {
            if(id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            MenuItem menuItem = _dbContext.MenuItems.FirstOrDefault(u => u.Id == id);
            if(menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

    }
}
