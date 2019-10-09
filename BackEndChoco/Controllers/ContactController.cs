using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BackEndChoco.Entities;
using BackEndChoco.Helpers;
using BackEndChoco.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BackEndChoco.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    //[ApiController]
    public class ContactController : ControllerBase
    {
        private readonly EFContext dbcontext;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public ContactController(EFContext _dbcontext, IConfiguration configuration, IHostingEnvironment env)
        {
            
            dbcontext = _dbcontext;
            _configuration = configuration;
            _env = env;
        }

        [HttpPost("addcontact")]
        public IActionResult AddContact([FromBody]ContactViewModel model)
        {
           
            if(model!=null)
            {
                string imageName = Guid.NewGuid().ToString() + ".jpg";
                string base64 = model.Image;
                if (base64.Contains(","))
                {
                    base64 = base64.Split(',')[1];
                }

                var bmp = base64.FromBase64StringToImage();
                string fileDestDir = _env.ContentRootPath;
                fileDestDir = Path.Combine(fileDestDir, _configuration.GetValue<string>("ImagesPath"));

                string fileSave = Path.Combine(fileDestDir, imageName);
                if (bmp != null)
                {
                    int size = 1000;
                    var image = ImageHelper.CompressImage(bmp, size, size);
                    image.Save(fileSave, ImageFormat.Jpeg);
                }
                dbcontext.Contacts.Add(new Contact { Name=model.Name,Company=model.Company,Office=model.Office,Image = imageName,PhoneNumber=model.PhoneNumber});
                dbcontext.SaveChanges();
                return Ok();
            }

            else
                return new BadRequestObjectResult("Server error");
        }


        [HttpGet("getcontacts")]
        public ICollection<ContactViewModel> GetContacts()
        {
            List<ContactViewModel> contacts = new List<ContactViewModel>();
            foreach (var c in dbcontext.Contacts)
            {
                contacts.Add
                (
                    new ContactViewModel
                    {
                        Name = c.Name,
                        Company = c.Company,
                        Office = c.Office,
                        Email = c.Email,
                        Image = c.Image,
                        PhoneNumber = c.PhoneNumber
                    }
                );
            };
            return contacts;
        }



    }
}