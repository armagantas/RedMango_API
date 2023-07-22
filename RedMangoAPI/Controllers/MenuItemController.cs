using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using RedMangoAPI.Models.DTO;
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


        [HttpGet("{id:int}", Name = "GetMenuItemById")]
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


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItemCreateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemCreateDTO.File == null || menuItemCreateDTO.File.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreateDTO.File.FileName)}";

                    string uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "RedMangoAPIImages");

                    Directory.CreateDirectory(uploadFolderPath);

                    string imagePath = Path.Combine(uploadFolderPath, fileName);

                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await menuItemCreateDTO.File.CopyToAsync(fileStream);
                    }

                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuItemCreateDTO.Name,
                        Price = menuItemCreateDTO.Price,
                        Category = menuItemCreateDTO.Category,
                        SpecialTag = menuItemCreateDTO.SpecialTag,
                        Description = menuItemCreateDTO.Description,
                        Image = imagePath,
                    };
                    _dbContext.MenuItems.Add(menuItemToCreate);
                    _dbContext.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItemById", new { id = menuItemToCreate.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                }

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

    }
}
