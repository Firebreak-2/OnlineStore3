namespace OnlineStore.Core;

public record ClientResponse(ClientResponseStatus Status, string? Message);

public enum ClientResponseStatus
{
    OK,    // success
    OKERR, // invalid request but valid path
    ERROR, // something unexpectedly went wrong
}