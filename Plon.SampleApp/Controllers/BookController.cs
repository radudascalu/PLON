using Plon.SampleApp.Models;
using System.Web.Http;

namespace Plon.SampleApp.Controllers
{
    public class BookController : ApiController
    {
        public IHttpActionResult Get(int id)
        {
            return Ok(new Book { Id = id, Name = "Abc" });
        }
    }
}
