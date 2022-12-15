using Microsoft.AspNetCore.Mvc;
using Tryitter.Context;
using Tryitter.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Tryitter.Controllers
{
  // [Authorize(AuthenticationSchemes = "Bearer")]
  [ApiController]
  [Route("[controller]")]
  public class PostagensController : ControllerBase
  {
    private readonly AppDbContext _context;

    public PostagensController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Postagem>> Get()
    {
      var postagens = _context.Postagens.ToList();
      if (postagens is null) return NotFound("Postagens não encontradas.");
      return postagens;
    }

    [HttpGet("{id:int}", Name="ObterPostagem")]
    public ActionResult<Postagem> Get(int id)
    {
      var postagem = _context.Postagens.FirstOrDefault(p => p.PostagemId == id);
      if (postagem is null) return NotFound("Postagem não encontrada.");
      return postagem;
    }

    [HttpPost]
    public ActionResult Post(Postagem postagem)
    {
      if (postagem is null) return BadRequest();

      _context.Postagens.Add(postagem);
      _context.SaveChanges();

      return new CreatedAtRouteResult("ObterPostagem", new { id = postagem.PostagemId }, postagem);
    }

    [HttpPut("{id:int}")]
    public ActionResult Put(int id, Postagem postagem)
    {
      if (id != postagem.PostagemId) return BadRequest();

      _context.Entry(postagem).State = EntityState.Modified;
      _context.SaveChanges();

      return Ok(postagem);
    }

    [HttpDelete("{id:int}")]
    public ActionResult<Postagem> Delete(int id)
    {
      var postagem = _context.Postagens.FirstOrDefault(p => p.PostagemId == id);
      if (postagem is null) return NotFound("Postagem não localizada.");
      
      _context.Postagens.Remove(postagem);
      _context.SaveChanges();

      return Ok(postagem);
    }
  }
}