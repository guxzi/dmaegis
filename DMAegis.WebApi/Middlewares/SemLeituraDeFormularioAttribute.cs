using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DMAegis.WebApi.Middlewares;

/// <summary>
/// Impede o model binding de ler o formulário (multipart) antes da action. A action lê o
/// formulário sozinha; assim, um upload grande demais chega ao GlobalExceptionHandler (413 com envelope)
/// em vez de virar um erro genérico de ModelState.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SemLeituraDeFormularioAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var fabricas = context.ValueProviderFactories;
        fabricas.RemoveType<FormValueProviderFactory>();
        fabricas.RemoveType<FormFileValueProviderFactory>();
        fabricas.RemoveType<JQueryFormValueProviderFactory>();
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}
