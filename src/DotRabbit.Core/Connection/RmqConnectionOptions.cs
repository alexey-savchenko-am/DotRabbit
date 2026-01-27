namespace DotRabbit.Core.Connection;

public sealed record RmqConnectionOptions(string Host, int Port, string User, string Password);