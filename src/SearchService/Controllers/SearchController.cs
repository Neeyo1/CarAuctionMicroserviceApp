using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Controllers;

public class SearchController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> SearchItems(string? searchTerm)
    {
        var query = DB.Find<Item>();

        query.Sort(x => x.Ascending(y => y.Make));

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query.Match(Search.Full, searchTerm).SortByTextScore();
        }

        var result = await query.ExecuteAsync();
        
        return Ok(result);
    }
}
