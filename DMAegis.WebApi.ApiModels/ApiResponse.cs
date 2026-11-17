namespace DMAegis.WebApi.ApiModels;

/// <summary>
/// Envelope padrão de todas as respostas da API: { responseCode, data, errorMessage }.
/// </summary>
public class ApiResponse<T>
{
    public int ResponseCode { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public static ApiResponse<T> Sucesso(T data) =>
        new() { ResponseCode = ResponseCodes.SUCESSO, Data = data };

    public static ApiResponse<T> Erro(int code, string mensagem) =>
        new() { ResponseCode = code, ErrorMessage = mensagem };
}
