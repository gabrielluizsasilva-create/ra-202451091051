# Proposta — Trabalho Semestral · Arquitetura de Aplicações Web 2026.2

- **Aluno:** Gabriel Luiz Sa Silva · **Curso/Turma:** Ciência da Computação / Newton Paiva
- **Repositório:** (https://github.com/gabrielluizsasilva)

## 1. Domínio e problema
Aplicação web de gerenciamento de e-commerce chamada **Saci Tech**, voltada para o controle simplificado de estoque e catálogo de produtos por categorias para pequenas e médias lojas online.

## 2. Entidades
- **Entidade A (Categoria):** `id`, `nome`, `descricao`, `ativa`
- **Entidade B (Produto):** `id`, `nome`, `preco`, `estoque`, `categoriaId`, `status`
- **Relacionamento:** 1-N (Uma Categoria possui N Produtos). Será modelado por **referência NoSQL** (`categoriaId` guardado no documento do produto), permitindo consultas independentes e listagens otimizadas sem duplicação de dados de categoria.

## 3. Endpoints previstos
### Categorias
- `GET /categorias` - Listar todas
- `GET /categorias/:id` - Detalhes por ID
- `POST /categorias` - Criar categoria
- `PUT /categorias/:id` - Atualizar categoria
- `DELETE /categorias/:id` - Remover categoria

### Produtos
- `GET /produtos` - Listar produtos
- `GET /produtos/:id` - Detalhes do produto
- `POST /produtos` - Criar produto
- `PUT /produtos/:id` - Atualizar produto
- `DELETE /produtos/:id` - Deletar produto

## 4. Regra de negócio
Ao cadastrar ou atualizar um produto, a camada de serviço valida se o preço é maior que zero e se a quantidade em estoque é não-negativa. Produtos com estoque zerado passam automaticamente para o status "Indisponível".

## 5. Casos de erro
1. Tentar buscar ou atualizar um produto/categoria com ID inexistente -> `404 Not Found`.
2. Tentar cadastrar um produto com preço menor ou igual a zero ou com categoria inexistente -> `400 Bad Request`.

## 6. Stack escolhida
- **Backend:** Node.js (Express)
- **Banco de Dados:** MongoDB (NoSQL)
- **Frontend:** HTML + JavaScript assíncrono (`fetch`)

## 7. Segurança e testes
- Autenticação via JWT com rotas protegidas por Bearer token.
- RBAC com dois perfis (`admin` e `usuario`), onde operações de modificação/exclusão requerem `admin`.
- Testes unitários com Jest cobrindo 2 cenários de sucesso e 2 de erro nas regras de negócio.