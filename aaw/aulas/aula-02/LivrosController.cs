using BibliotecaApi.Models;
using BibliotecaApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivrosController : ControllerBase
{
    private readonly LivroRepository _repository;

    public LivrosController(LivroRepository repository)
    {
        _repository = repository;
    }

    // PASSO 1 — GET api/livros
    // Retorna 200 OK com a lista completa de livros.
    [HttpGet]
    public ActionResult<List<Livro>> GetAll()
    {
        return Ok(_repository.GetAll());
    }

    // PASSO 2 — GET api/livros/{id}
    // Retorna 200 OK se encontrar o livro.
    // Retorna 404 Not Found se o livro não existir.
    [HttpGet("{id}")]
    public ActionResult<Livro> GetById(int id)
    {
        var livro = _repository.GetById(id);

        if (livro is null)
        {
            return NotFound();
        }

        return Ok(livro);
    }

    // PASSO 3 — POST api/livros
    // Cria um novo livro.
    // Retorna 201 Created com o header Location se der certo.
    // Retorna 400 Bad Request se o título não for informado.
    [HttpPost]
    public ActionResult<Livro> Create(Livro livro)
    {
        if (livro is null || string.IsNullOrWhiteSpace(livro.Titulo))
        {
            return BadRequest(new
            {
                title = "Bad Request",
                status = 400,
                errors = new
                {
                    Titulo = new[] { "O campo Titulo é obrigatório" }
                }
            });
        }

        var criado = _repository.Create(livro);

        return CreatedAtAction(
            nameof(GetById),
            new { id = criado.Id },
            criado
        );
    }

    // PASSO 4 — PUT api/livros/{id}
    // Atualiza completamente os dados de um livro existente.
    // Retorna 200 OK se atualizar.
    // Retorna 404 Not Found se o livro não existir.
    // Retorna 400 Bad Request se o título não for informado.
    [HttpPut("{id}")]
    public ActionResult<Livro> Update(int id, Livro livro)
    {
        if (livro is null || string.IsNullOrWhiteSpace(livro.Titulo))
        {
            return BadRequest(new
            {
                title = "Bad Request",
                status = 400,
                errors = new
                {
                    Titulo = new[] { "O campo Titulo é obrigatório" }
                }
            });
        }

        var atualizado = _repository.Update(id, livro);

        if (atualizado is null)
        {
            return NotFound();
        }

        return Ok(atualizado);
    }

    // PASSO 5 — DELETE api/livros/{id}
    // Remove um livro pelo ID.
    // Retorna 204 No Content se remover com sucesso.
    // Retorna 404 Not Found se o livro não existir.
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var removido = _repository.Delete(id);

        if (!removido)
        {
            return NotFound();
        }

        return NoContent();
    }
}