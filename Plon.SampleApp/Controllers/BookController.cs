using Plon.SampleApp.Models;
using PLON.Core;
using System.Web.Http;

namespace Plon.SampleApp.Controllers
{
    public class BookController : ApiController
    {
        [PlonModel(typeof(Book))]
        public IHttpActionResult Get(int id)
        {
            return Ok(new Book { Id = id, Name = "Abc" });
        }
    }
}
